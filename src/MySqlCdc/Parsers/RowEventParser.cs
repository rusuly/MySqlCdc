using System;
using System.Collections;
using System.Collections.Generic;
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
        protected int RowsEventVersion { get; }
        protected Dictionary<int, IColumnParser> ColumnParsers { get; }
        protected Dictionary<long, TableMapEvent> TableMapCache { get; }

        protected RowEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
        {
            TableMapCache = tableMapCache;
            RowsEventVersion = rowsEventVersion;
            ColumnParsers = new Dictionary<int, IColumnParser>();

            /* Numeric types. The only place where numbers can be negative */
            ColumnParsers[(int)ColumnType.BIT] = new BitParser();
            ColumnParsers[(int)ColumnType.TINY] = new TinyIntParser();
            ColumnParsers[(int)ColumnType.SHORT] = new SmallIntParser();
            ColumnParsers[(int)ColumnType.INT24] = new MediumIntParser();
            ColumnParsers[(int)ColumnType.LONG] = new IntParser();
            ColumnParsers[(int)ColumnType.LONGLONG] = new BigIntParser();
            ColumnParsers[(int)ColumnType.FLOAT] = new FloatParser();
            ColumnParsers[(int)ColumnType.DOUBLE] = new DoubleParser();
            ColumnParsers[(int)ColumnType.NEWDECIMAL] = new NewDecimalParser();

            /* String types, includes varchar, varbinary & fixed char, binary */
            ColumnParsers[(int)ColumnType.STRING] = new StringParser();
            ColumnParsers[(int)ColumnType.VARCHAR] = new StringParser();
            ColumnParsers[(int)ColumnType.VAR_STRING] = new StringParser();

            /* ENUM, SET types */            
            ColumnParsers[(int)ColumnType.ENUM] = new EnumParser();
            ColumnParsers[(int)ColumnType.SET] = new SetParser();

            /* Blob types. MariaDB always creates BLOB for first three */
            ColumnParsers[(int)ColumnType.TINY_BLOB] = new BlobParser();
            ColumnParsers[(int)ColumnType.MEDIUM_BLOB] = new BlobParser();
            ColumnParsers[(int)ColumnType.LONG_BLOB] = new BlobParser();
            ColumnParsers[(int)ColumnType.BLOB] = new BlobParser();

            /* Date and time types */
            ColumnParsers[(int)ColumnType.YEAR] = new YearParser();
            ColumnParsers[(int)ColumnType.DATE] = new DateParser();
            ColumnParsers[(int)ColumnType.TIME] = new TimeParser();
            ColumnParsers[(int)ColumnType.TIMESTAMP] = new TimeStampParser();
            ColumnParsers[(int)ColumnType.DATETIME] = new DateTimeParser();

            // MySQL 5.6.4+ types. Supported in MariaDB.
            ColumnParsers[(int)ColumnType.TIME2] = new Time2Parser();
            ColumnParsers[(int)ColumnType.TIMESTAMP2] = new TimeStamp2Parser();
            ColumnParsers[(int)ColumnType.DATETIME2] = new DateTime2Parser();

            /* MySQL-specific data types */
            ColumnParsers[(int)ColumnType.GEOMETRY] = new BlobParser();
            ColumnParsers[(int)ColumnType.JSON] = new BlobParser();
        }

        protected (long tableId, int flags, int columnsNumber) ParseHeader(ref PacketReader reader)
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

        protected ColumnData ParseRow(ref PacketReader reader, long tableId, BitArray columnsPresent)
        {
            if (!TableMapCache.TryGetValue(tableId, out var tableMap))
                throw new InvalidOperationException("No preceding TableMapEvent event was found for the row event. You possibly started replication in the middle of logical event group.");

            var row = new object[tableMap.ColumnTypes.Length];
            var cellsNumber = GetBitsNumber(columnsPresent);
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
            if (ColumnParsers.TryGetValue(columnType, out var columnParser))
                return columnParser.ParseColumn(ref reader, metadata);

            throw new InvalidOperationException($"Column type {columnType} is not supported");
        }

        private int GetBitsNumber(BitArray bitmap)
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
