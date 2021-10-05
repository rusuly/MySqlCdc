using System;
using System.Collections.Generic;
using MySqlCdc.Columns;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Base class for parsing row based events.
/// See <a href="https://mariadb.com/kb/en/library/rows_event_v1/">MariaDB rows version 1</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/rows-event.html#write-rows-eventv2">MySQL rows version 1/2</a>
/// See <a href="https://github.com/shyiko/mysql-binlog-connector-java">AbstractRowsEventDataDeserializer</a>
/// </summary>
public abstract class RowEventParser
{
    private readonly ColumnParser _columnParser = new ();

    private int RowsEventVersion { get; }
    private Dictionary<long, TableMapEvent> TableMapCache { get; }
    
    /// <summary>
    /// Creates a new <see cref="RowEventParser"/>.
    /// </summary>
    protected RowEventParser(int rowsEventVersion, Dictionary<long, TableMapEvent> tableMapCache)
    {
        RowsEventVersion = rowsEventVersion;
        TableMapCache = tableMapCache;
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
    /// Parses rows for delete and write events.
    /// </summary>
    protected IReadOnlyList<RowData> ParseRowDataList(ref PacketReader reader, long tableId, bool[] columnsPresent)
    {
        var cellsIncluded = GetBitsNumber(columnsPresent);
        if (!TableMapCache.TryGetValue(tableId, out var tableMap))
            throw new InvalidOperationException(EventConstants.TableMapNotFound);

        var rows = new List<RowData>();
        while (!reader.IsEmpty())
        {
            rows.Add(ParseRow(ref reader, tableMap, columnsPresent, cellsIncluded));
        }
        return rows;
    }
    
    /// <summary>
    /// Parses rows for update events.
    /// </summary>
    protected IReadOnlyList<UpdateRowData> ParseUpdatedRows(
        ref PacketReader reader,
        long tableId,
        bool[] columnsBeforeUpdate,
        bool[] columnsAfterUpdate)
    {
        var cellsIncludedBeforeUpdate = GetBitsNumber(columnsBeforeUpdate);
        var cellsIncludedAfterUpdate = GetBitsNumber(columnsAfterUpdate);
        if (!TableMapCache.TryGetValue(tableId, out var tableMap))
            throw new InvalidOperationException(EventConstants.TableMapNotFound);

        var rows = new List<UpdateRowData>();
        while (!reader.IsEmpty())
        {
            var rowBeforeUpdate = ParseRow(ref reader, tableMap, columnsBeforeUpdate, cellsIncludedBeforeUpdate);
            var rowAfterUpdate = ParseRow(ref reader, tableMap, columnsAfterUpdate, cellsIncludedAfterUpdate);

            rows.Add(new UpdateRowData(rowBeforeUpdate, rowAfterUpdate));
        }
        return rows;
    }
    
    /// <summary>
    /// Parses a row in a rows event.
    /// </summary>
    protected RowData ParseRow(ref PacketReader reader, TableMapEvent tableMap, bool[] columnsPresent, int cellsIncluded)
    {
        var row = new object?[tableMap.ColumnTypes.Length];
        var nullBitmap = reader.ReadBitmapLittleEndian(cellsIncluded);

        int skippedColumns = 0;
        for (int i = 0; i < tableMap.ColumnTypes.Length; i++)
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

                if (columnType == (int)ColumnType.String)
                {
                    GetActualStringType(ref columnType, ref metadata);
                }
                row[i] = ParseCell(ref reader, columnType, metadata);
            }
        }
        return new RowData(row);
    }

    private object? ParseCell(ref PacketReader reader, int columnType, int metadata)
    {
        return (ColumnType)columnType switch
        {
            /* Numeric types. The only place where numbers can be negative */
            ColumnType.Tiny => _columnParser.ParseTinyInt(ref reader, metadata),
            ColumnType.Short => _columnParser.ParseSmallInt(ref reader, metadata),
            ColumnType.Int24 => _columnParser.ParseMediumInt(ref reader, metadata),
            ColumnType.Long => _columnParser.ParseInt(ref reader, metadata),
            ColumnType.LongLong => _columnParser.ParseBigInt(ref reader, metadata),
            ColumnType.Float => _columnParser.ParseFloat(ref reader, metadata),
            ColumnType.Double => _columnParser.ParseDouble(ref reader, metadata),
            ColumnType.NewDecimal => _columnParser.ParseNewDecimal(ref reader, metadata),

            /* String types, includes varchar, varbinary & fixed char, binary */
            ColumnType.String => _columnParser.ParseString(ref reader, metadata),
            ColumnType.VarChar => _columnParser.ParseString(ref reader, metadata),
            ColumnType.VarString => _columnParser.ParseString(ref reader, metadata),

            /* BIT, ENUM, SET types */
            ColumnType.Bit => _columnParser.ParseBit(ref reader, metadata),
            ColumnType.Enum => _columnParser.ParseEnum(ref reader, metadata),
            ColumnType.Set => _columnParser.ParseSet(ref reader, metadata),

            /* Blob types. MariaDB always creates BLOB for first three */
            ColumnType.TinyBlob => _columnParser.ParseBlob(ref reader, metadata),
            ColumnType.MediumBlob => _columnParser.ParseBlob(ref reader, metadata),
            ColumnType.LongBlob => _columnParser.ParseBlob(ref reader, metadata),
            ColumnType.Blob => _columnParser.ParseBlob(ref reader, metadata),

            /* Date and time types */
            ColumnType.Year => _columnParser.ParseYear(ref reader, metadata),
            ColumnType.Date => _columnParser.ParseDate(ref reader, metadata),

            // Older versions of MySQL.
            ColumnType.Time => _columnParser.ParseTime(ref reader, metadata),
            ColumnType.Timestamp => _columnParser.ParseTimeStamp(ref reader, metadata),
            ColumnType.DateTime => _columnParser.ParseDateTime(ref reader, metadata),

            // MySQL 5.6.4+ types. Supported from MariaDB 10.1.2.
            ColumnType.Time2 => _columnParser.ParseTime2(ref reader, metadata),
            ColumnType.TimeStamp2 => _columnParser.ParseTimeStamp2(ref reader, metadata),
            ColumnType.DateTime2 => _columnParser.ParseDateTime2(ref reader, metadata),

            /* MySQL-specific data types */
            ColumnType.Geometry => _columnParser.ParseBlob(ref reader, metadata),
            ColumnType.Json => _columnParser.ParseBlob(ref reader, metadata),
            _ => throw new InvalidOperationException($"Column type {columnType} is not supported")
        };
    }

    /// <summary>
    /// Gets number of bits set in a bitmap.
    /// </summary>
    protected int GetBitsNumber(bool[] bitmap)
    {
        // USING LINQ HERE WILL SLOW DOWN PERFORMANCE A LOT
        int value = 0;
        for (int i = 0; i < bitmap.Length; i++)
        {
            if (bitmap[i])
                value++;
        }
        return value;
    }

    /// <summary>
    /// Parses actual string type
    /// See: https://bugs.mysql.com/bug.php?id=37426
    /// See: https://github.com/mysql/mysql-server/blob/9c3a49ec84b521cb0b35383f119099b2eb25d4ff/sql/log_event.cc#L1988
    /// </summary>
    public static void GetActualStringType(ref int columnType, ref int metadata)
    {
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
            if (byte0 == (byte)ColumnType.Enum || byte0 == (byte)ColumnType.Set)
            {
                columnType = byte0;
            }
            metadata = byte1;
        }
    }
}