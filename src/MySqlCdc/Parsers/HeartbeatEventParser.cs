using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="HeartbeatEvent"/> events.
/// Supports all versions of MariaDB and MySQL.
/// </summary>
public class HeartbeatEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="HeartbeatEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var binlogFilename = reader.ReadStringToEndOfFile();

        return new HeartbeatEvent(header, binlogFilename);
    }
}