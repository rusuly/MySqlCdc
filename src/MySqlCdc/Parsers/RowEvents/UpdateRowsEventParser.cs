using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="UpdateRowsEvent"/> events.
/// Supports all versions of MariaDB and MySQL 5.5+ (V1 and V2 row events).
/// </summary>
public class UpdateRowsEventParser : RowEventParser, IEventParser
{
    /// <summary>
    /// Creates a new <see cref="UpdateRowsEventParser"/>.
    /// </summary>
    public UpdateRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
        : base(rowsEventVersion, tableMapCache)
    {
    }

    /// <summary>
    /// Parses <see cref="UpdateRowsEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var shared = ParseHeader(ref reader);

        var columnsBeforeUpdate = reader.ReadBitmapLittleEndian(shared.columnsNumber);
        var columnsAfterUpdate = reader.ReadBitmapLittleEndian(shared.columnsNumber);

        var rows = ParseUpdatedRows(ref reader, shared.tableId, columnsBeforeUpdate, columnsAfterUpdate);
        return new UpdateRowsEvent(shared.tableId, shared.flags, shared.columnsNumber, columnsBeforeUpdate, columnsAfterUpdate, rows);
    }
}