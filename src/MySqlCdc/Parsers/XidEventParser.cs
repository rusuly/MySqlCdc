using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="XidEvent"/> events.
/// Supports all versions of MariaDB and MySQL.
/// </summary>
public class XidEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="XidEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var xid = reader.ReadInt64LittleEndian();

        return new XidEvent(xid);
    }
}