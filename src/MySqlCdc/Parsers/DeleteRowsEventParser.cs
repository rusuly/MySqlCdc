using System;
using System.Collections.Generic;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="DeleteRowsEvent"/> events.
/// Supports all versions of MariaDB and MySQL 5.5+ (V1 and V2 row events).
/// </summary>
public class DeleteRowsEventParser : RowEventParser, IEventParser
{
    private Dictionary<long, TableMapEvent> _tableMapCache { get; }

    /// <summary>
    /// Creates a new <see cref="DeleteRowsEventParser"/>.
    /// </summary>
    public DeleteRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
        : base(rowsEventVersion)
    {
        _tableMapCache = tableMapCache;
    }

    /// <summary>
    /// Parses <see cref="DeleteRowsEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var shared = ParseHeader(ref reader);

        var columnsPresent = reader.ReadBitmapLittleEndian(shared.columnsNumber);
        var rows = ParseDeleteRows(ref reader, shared.tableId, columnsPresent);

        return new DeleteRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsPresent, rows);
    }

    private IReadOnlyList<RowData> ParseDeleteRows(ref PacketReader reader, long tableId, bool[] columnsPresent)
    {
        var cellsIncluded = GetBitsNumber(columnsPresent);
        if (!_tableMapCache.TryGetValue(tableId, out var tableMap))
            throw new InvalidOperationException(EventConstants.TableMapNotFound);

        var rows = new List<RowData>();
        while (!reader.IsEmpty())
        {
            rows.Add(ParseRow(ref reader, tableMap, columnsPresent, cellsIncluded));
        }
        return rows;
    }
}