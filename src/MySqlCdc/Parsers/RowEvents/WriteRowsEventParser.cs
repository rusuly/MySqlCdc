using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="WriteRowsEvent"/> events.
/// Supports all versions of MariaDB and MySQL 5.5+ (V1 and V2 row events).
/// </summary>
public class WriteRowsEventParser : RowEventParser, IEventParser
{
    /// <summary>
    /// Creates a new <see cref="WriteRowsEventParser"/>.
    /// </summary>
    public WriteRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
        : base(rowsEventVersion, tableMapCache)
    {
    }

    /// <summary>
    /// Parses <see cref="WriteRowsEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var shared = ParseHeader(ref reader);

        var columnsPresent = reader.ReadBitmapLittleEndian(shared.columnsNumber);
        var rows = ParseRowDataList(ref reader, shared.tableId, columnsPresent);

        return new WriteRowsEvent(shared.tableId, shared.flags, shared.columnsNumber, columnsPresent, rows);
    }
}