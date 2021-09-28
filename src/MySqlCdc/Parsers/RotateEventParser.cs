using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="RotateEvent"/> events.
/// Supports all versions of MariaDB and MySQL.
/// </summary>
public class RotateEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="RotateEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var binlogPosition = reader.ReadInt64LittleEndian();
        var binlogFilename = reader.ReadStringToEndOfFile();

        return new RotateEvent(header, binlogFilename, binlogPosition);
    }
}