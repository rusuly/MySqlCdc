using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MariaDb;

/// <summary>
/// Parses <see cref="RowsQueryEvent"/> events in MariaDB 5.3+.
/// </summary>
public class AnnotateRowsEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="RowsQueryEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var query = reader.ReadStringToEndOfFile();
        return new RowsQueryEvent(query);
    }
}