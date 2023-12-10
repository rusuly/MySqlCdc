using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="RowsQueryEvent"/> events.
/// Supports all versions of MariaDB and MySQL.
/// </summary>
public class RowsQueryEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="RowsQueryEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var queryLength = reader.ReadIntLittleEndian(1);
        var query = reader.ReadString(queryLength);
        return new RowsQueryEvent(query);
    }
}