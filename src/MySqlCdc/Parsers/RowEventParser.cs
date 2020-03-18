using System;
using MySqlCdc.Columns;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Base class for parsing row based events.
    /// See <a href="https://mariadb.com/kb/en/library/rows_event_v1/">MariaDB rows version 1</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/rows-event.html#write-rows-eventv2">MySQL rows version 1/2</a>
    /// See <a href="https://github.com/shyiko/mysql-binlog-connector-java">AbstractRowsEventDataDeserializer</a>
    /// </summary>
    public abstract class RowEventParser
    {
        private readonly ColumnParser _columnParser = new ColumnParser();

        /// <summary>
        /// Gets rows event version to determine row format.
        /// </summary>
        protected int RowsEventVersion { get; }

        /// <summary>
        /// Creates a new <see cref="RowEventParser"/>.
        /// </summary>
        protected RowEventParser(int rowsEventVersion)
        {
            RowsEventVersion = rowsEventVersion;
        }

        /// <summary>
        /// Parses the header in a rows event.
        /// </summary>
        protected (long tableId, int flags, int columnsNumber) ParseHeader(ref PacketReader reader)
        {
            long tableId = reader.ReadLongLittleEndian(6);
            int flags = reader.ReadUInt16LittleEndian();

            // Ignore extra data from newer versions of events
            if (RowsEventVersion == 2)
            {
                int extraDataLength = reader.ReadUInt16LittleEndian();
                reader.Advance(extraDataLength - 2);
            }

            var columnsNumber = (int)reader.ReadLengthEncodedNumber();
            return (tableId, flags, columnsNumber);
        }

        /// <summary>
        /// Parses a row in a rows event.
        /// </summary>
        protected ColumnData ParseRow(ref PacketReader reader, TableMapEvent tableMap, bool[] columnsPresent, int cellsIncluded)
        {
            var row = new object[tableMap.ColumnTypes.Length];
            var nullBitmap = reader.ReadBitmapLittleEndian(cellsIncluded);

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
                    int columnType = tableMap.ColumnTypes[i];
                    int metadata = tableMap.ColumnMetadata[i];

                    if (columnType == (int)ColumnType.STRING)
                    {
                        GetActualStringType(ref columnType, ref metadata);
                    }
                    row[i] = ParseCell(ref reader, columnType, metadata);
                }
            }
            return new ColumnData(row);
        }

        private object ParseCell(ref PacketReader reader, int columnType, int metadata)
        {
            return (ColumnType)columnType switch
            {
                /* Numeric types. The only place where numbers can be negative */
                ColumnType.BIT => _columnParser.ParseBit(ref reader, metadata),
                ColumnType.TINY => _columnParser.ParseTinyInt(ref reader, metadata),
                ColumnType.SHORT => _columnParser.ParseSmallInt(ref reader, metadata),
                ColumnType.INT24 => _columnParser.ParseMediumInt(ref reader, metadata),
                ColumnType.LONG => _columnParser.ParseInt(ref reader, metadata),
                ColumnType.LONGLONG => _columnParser.ParseBigInt(ref reader, metadata),
                ColumnType.FLOAT => _columnParser.ParseFloat(ref reader, metadata),
                ColumnType.DOUBLE => _columnParser.ParseDouble(ref reader, metadata),
                ColumnType.NEWDECIMAL => _columnParser.ParseNewDecimal(ref reader, metadata),

                /* String types, includes varchar, varbinary & fixed char, binary */
                ColumnType.STRING => _columnParser.ParseString(ref reader, metadata),
                ColumnType.VARCHAR => _columnParser.ParseString(ref reader, metadata),
                ColumnType.VAR_STRING => _columnParser.ParseString(ref reader, metadata),

                /* ENUM, SET types */
                ColumnType.ENUM => _columnParser.ParseEnum(ref reader, metadata),
                ColumnType.SET => _columnParser.ParseSet(ref reader, metadata),

                /* Blob types. MariaDB always creates BLOB for first three */
                ColumnType.TINY_BLOB => _columnParser.ParseBlob(ref reader, metadata),
                ColumnType.MEDIUM_BLOB => _columnParser.ParseBlob(ref reader, metadata),
                ColumnType.LONG_BLOB => _columnParser.ParseBlob(ref reader, metadata),
                ColumnType.BLOB => _columnParser.ParseBlob(ref reader, metadata),

                /* Date and time types */
                ColumnType.YEAR => _columnParser.ParseYear(ref reader, metadata),
                ColumnType.DATE => _columnParser.ParseDate(ref reader, metadata),
                ColumnType.TIME => _columnParser.ParseTime(ref reader, metadata),
                ColumnType.TIMESTAMP => _columnParser.ParseTimeStamp(ref reader, metadata),
                ColumnType.DATETIME => _columnParser.ParseDateTime(ref reader, metadata),

                // MySQL 5.6.4+ types. Supported in MariaDB.
                ColumnType.TIME2 => _columnParser.ParseTime2(ref reader, metadata),
                ColumnType.TIMESTAMP2 => _columnParser.ParseTimeStamp2(ref reader, metadata),
                ColumnType.DATETIME2 => _columnParser.ParseDateTime2(ref reader, metadata),

                /* MySQL-specific data types */
                ColumnType.GEOMETRY => _columnParser.ParseBlob(ref reader, metadata),
                ColumnType.JSON => _columnParser.ParseBlob(ref reader, metadata),
                _ => throw new InvalidOperationException($"Column type {columnType} is not supported")
            };
        }

        /// <summary>
        /// Gets number of bits set in a bitmap.
        /// </summary>
        protected int GetBitsNumber(bool[] bitmap)
        {
            int value = 0;
            for (int i = 0; i < bitmap.Length; i++)
            {
                if (bitmap[i])
                    value++;
            }
            return value;
        }

        private void GetActualStringType(ref int columnType, ref int metadata)
        {
            // See: https://bugs.mysql.com/bug.php?id=37426
            // See: https://github.com/mysql/mysql-server/blob/9c3a49ec84b521cb0b35383f119099b2eb25d4ff/sql/log_event.cc#L1988            

            // CHAR column type
            if (metadata < 256)
                return;

            // CHAR or ENUM or SET column types
            int byte0 = metadata >> 8;
            int byte1 = metadata & 0xFF;

            if ((byte0 & 0x30) != 0x30)
            {
                /* a long CHAR() field: see #37426 */
                metadata = byte1 | (((byte0 & 0x30) ^ 0x30) << 4);
                columnType = byte0 | 0x30;
            }
            else
            {
                if (byte0 == (byte)ColumnType.ENUM || byte0 == (byte)ColumnType.SET)
                {
                    columnType = byte0;
                }
                metadata = byte1;
            }
        }
    }
}
