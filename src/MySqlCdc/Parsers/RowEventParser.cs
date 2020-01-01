using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Base class for parsing row based events.
    /// <see cref="https://mariadb.com/kb/en/library/rows_event_v1/"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/rows-event.html#write-rows-eventv2"/>
    /// See AbstractRowsEventDataDeserializer from <see cref="https://github.com/shyiko/mysql-binlog-connector-java"/>
    /// </summary>
    public abstract class RowEventParser : IEventParser
    {
        protected int RowsEventVersion { get; }
        protected Dictionary<long, TableMapEvent> TableMapCache { get; }

        protected RowEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
        {
            TableMapCache = tableMapCache;
            RowsEventVersion = rowsEventVersion;
        }

        public abstract IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer);

        protected (long tableId, int flags, int columnsNumber) ParseHeader(PacketReader reader)
        {
            var tableId = reader.ReadLong(6);
            var flags = reader.ReadInt(2);

            // Ignore extra data from newer versions of events
            if (RowsEventVersion == 2)
            {
                var extraDataLength = reader.ReadInt(2);
                reader.Skip(extraDataLength - 2);
            }

            var columnsNumber = (int)reader.ReadLengthEncodedNumber();
            return (tableId, flags, columnsNumber);
        }

        protected ColumnData ParseRow(PacketReader reader, long tableId, BitArray columnsPresent)
        {
            if (!TableMapCache.TryGetValue(tableId, out var tableMap))
                throw new InvalidOperationException("No preceding TableMapEvent event was found for the row event. You possibly started replication in the middle of logical event group.");

            var row = new object[tableMap.ColumnTypes.Length];
            var cellsNumber = columnsPresent.Cast<bool>().Where(x => x).Count();
            var nullBitmap = reader.ReadBitmap(cellsNumber);

            for (int i = 0, skippedColumns = 0; i < tableMap.ColumnTypes.Length; i++)
            {
                // Data is missing if binlog_row_image != full
                if (!columnsPresent[i])
                {
                    skippedColumns++;
                    continue;
                }

                int nullBitmapIndex = i - skippedColumns;
                if (!nullBitmap[nullBitmapIndex])
                {
                    int columnType = tableMap.ColumnTypes[i],
                        metadata = tableMap.ColumnMetadata[i],
                        length = 0;

                    if (columnType == (int)ColumnType.STRING)
                    {
                        GetActualStringType(ref columnType, ref metadata, ref length);
                    }
                    row[i] = ParseCell(reader, columnType, metadata, length);
                }
            }
            return new ColumnData(row);
        }

        private object ParseCell(PacketReader reader, int columnType, int metadata, int length)
        {
            return (ColumnType)columnType switch
            {
                /* Numeric types. The only place where numbers can be negative */
                ColumnType.DECIMAL => ParseDecimal(reader, metadata),
                ColumnType.BIT => ParseBit(reader, metadata),
                ColumnType.TINY => (reader.ReadInt(1) << 24) >> 24,
                ColumnType.SHORT => (reader.ReadInt(2) << 16) >> 16,
                ColumnType.INT24 => (reader.ReadInt(3) << 8) >> 8,
                ColumnType.LONG => reader.ReadInt(4),
                ColumnType.LONGLONG => reader.ReadLong(8),
                ColumnType.FLOAT => BitConverter.ToSingle(BitConverter.GetBytes(reader.ReadInt(4)), 0),
                ColumnType.DOUBLE => BitConverter.ToDouble(BitConverter.GetBytes(reader.ReadLong(8)), 0),

                /* Variable strings, includes varchar  varbinary */
                ColumnType.VARCHAR => ParseVariableString(reader, metadata),
                ColumnType.VAR_STRING => ParseVariableString(reader, metadata),

                /* CHAR(BINARY), ENUM, SET types share same metadata */
                ColumnType.STRING => ParseFixedString(reader, length),
                ColumnType.ENUM => reader.ReadInt(length),
                ColumnType.SET => reader.ReadLong(length),

                /* Date and time types */
                ColumnType.YEAR => 1900 + reader.ReadInt(1),
                ColumnType.DATE => ParseDate(reader),

                ColumnType.TIME => ParseTime(reader),
                ColumnType.TIMESTAMP => ParseTimeStamp(reader),
                ColumnType.DATETIME => ParseDateTime(reader),

                ColumnType.TIME2 => ParseTime2(reader, metadata),
                ColumnType.TIMESTAMP2 => ParseTimeStamp2(reader, metadata),
                ColumnType.DATETIME2 => ParseDateTime2(reader, metadata),

                /* Blob types. MariaDB always creates BLOB for first three */
                ColumnType.TINY_BLOB => ParseBlob(reader, metadata),
                ColumnType.MEDIUM_BLOB => ParseBlob(reader, metadata),
                ColumnType.LONG_BLOB => ParseBlob(reader, metadata),
                ColumnType.BLOB => ParseBlob(reader, metadata),

                /* MySQL-specific data types */
                ColumnType.GEOMETRY => ParseBlob(reader, metadata),
                ColumnType.JSON => ParseBlob(reader, metadata),

                _ => throw new InvalidOperationException($"Column type is not supported")
            };
        }

        private BitArray ParseBit(PacketReader reader, int metadata)
        {
            int length = (metadata >> 8) * 8 + (metadata & 0xFF);
            return reader.ReadBitmapBigEndian(length);
        }

        private object ParseDecimal(PacketReader reader, int metadata)
        {
            // TODO: Implement Decimal type
            throw new NotImplementedException("Parsing Decimal type is not supported");
        }

        private string ParseVariableString(PacketReader reader, int metadata)
        {
            var length = metadata > 255 ? reader.ReadInt(2) : reader.ReadInt(1);
            return reader.ReadString(length);
        }

        private string ParseFixedString(PacketReader reader, int length)
        {
            length = length > 255 ? reader.ReadInt(2) : reader.ReadInt(1);
            return reader.ReadString(length);
        }

        private DateTime? ParseDate(PacketReader reader)
        {
            // Bits 1-5 store the day. Bits 6-9 store the month. The remaining bits store the year.
            int value = reader.ReadInt(3);
            int year = GetBitSliceValue(value, 0, 15, 24);
            int month = GetBitSliceValue(value, 15, 4, 24);
            int day = GetBitSliceValue(value, 19, 5, 24);

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day);
        }

        private TimeSpan ParseTime(PacketReader reader)
        {
            int value = reader.ReadInt(3);
            int seconds = value % 100;
            value = value / 100;
            int minutes = value % 100;
            value = value / 100;
            int hours = value;
            return new TimeSpan(hours, minutes, seconds);
        }

        private DateTimeOffset ParseTimeStamp(PacketReader reader)
        {
            long seconds = reader.ReadLong(4);
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        private DateTime? ParseDateTime(PacketReader reader)
        {
            long value = reader.ReadLong(8);
            int second = (int)value % 100;
            value = value / 100;
            int minute = (int)value % 100;
            value = value / 100;
            int hour = (int)value % 100;
            value = value / 100;
            int day = (int)value % 100;
            value = value / 100;
            int month = (int)value % 100;
            value = value / 100;
            int year = (int)value;

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day, hour, minute, second);
        }

        private TimeSpan ParseTime2(PacketReader reader, int metadata)
        {
            //  See https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html
            //  1 bit sign    (1 = non-negative, 0 = negative)
            //  1 bit unused  (reserved for future extensions)
            // 10 bits hour   (0-838)
            //  6 bits minute (0-59) 
            //  6 bits second (0-59) 
            //  ---------------------
            // 24 bits = 3 bytes

            long value = reader.ReadBigEndianLong(3);
            int millisecond = ParseFractionalPart(reader, metadata) / 1000;

            int hours = GetBitSliceValue(value, 2, 10, 24);
            int minutes = GetBitSliceValue(value, 12, 6, 24);
            int seconds = GetBitSliceValue(value, 18, 6, 24);

            return new TimeSpan(0, hours, minutes, seconds, millisecond);
        }

        private DateTimeOffset ParseTimeStamp2(PacketReader reader, int metadata)
        {
            long seconds = reader.ReadBigEndianLong(4);
            int millisecond = ParseFractionalPart(reader, metadata) / 1000;
            long timestamp = seconds * 1000 + millisecond;

            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }

        private DateTime? ParseDateTime2(PacketReader reader, int metadata)
        {
            //  See https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html
            //  1 bit  sign           (1 = non-negative, 0 = negative)
            // 17 bits year*13+month  (year 0-9999, month 0-12)
            //  5 bits day            (0-31)
            //  5 bits hour           (0-23)
            //  6 bits minute         (0-59)
            //  6 bits second         (0-59)
            //  ---------------------------
            // 40 bits = 5 bytes

            long value = reader.ReadBigEndianLong(5);
            int millisecond = ParseFractionalPart(reader, metadata) / 1000;

            int yearMonth = GetBitSliceValue(value, 1, 17, 40);
            int year = yearMonth / 13;
            int month = yearMonth % 13;
            int day = GetBitSliceValue(value, 18, 5, 40);
            int hour = GetBitSliceValue(value, 23, 5, 40);
            int minute = GetBitSliceValue(value, 28, 6, 40);
            int second = GetBitSliceValue(value, 34, 6, 40);

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        private int ParseFractionalPart(PacketReader reader, int metadata)
        {
            int length = (metadata + 1) / 2;
            if (length == 0)
                return 0;

            int fraction = reader.ReadBigEndianInt(length);
            return fraction * (int)Math.Pow(100, 3 - length);
        }

        private byte[] ParseBlob(PacketReader reader, int metadata)
        {
            var length = reader.ReadInt(metadata);
            return reader.ReadByteArraySlow(length);
        }

        private int GetBitSliceValue(long value, int startIndex, int length, int totalSize)
        {
            long result = value >> totalSize - (startIndex + length);
            return (int)(result & ((1 << length) - 1));
        }

        private void GetActualStringType(ref int columnType, ref int metadata, ref int length)
        {
            // See: https://bugs.mysql.com/bug.php?id=37426
            // See: https://github.com/mysql/mysql-server/blob/9c3a49ec84b521cb0b35383f119099b2eb25d4ff/sql/log_event.cc#L1988            

            // CHAR column type
            if (metadata < 256)
            {
                length = metadata;
                return;
            }

            // CHAR or ENUM or SET column types
            int byte0 = metadata >> 8;
            int byte1 = metadata & 0xFF;

            if ((byte0 & 0x30) != 0x30)
            {
                /* a long CHAR() field: see #37426 */
                length = byte1 | (((byte0 & 0x30) ^ 0x30) << 4);
                columnType = byte0 | 0x30;
            }
            else
            {
                if (byte0 == (byte)ColumnType.ENUM || byte0 == (byte)ColumnType.SET)
                {
                    columnType = byte0;
                }
                length = byte1;
            }
        }
    }
}
