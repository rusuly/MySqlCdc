using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MySql;

/// <summary>
/// Parses <see cref="GtidEvent"/> events in MySQL 5.6+.
/// </summary>
public class GtidEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="GtidEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        byte flags = reader.ReadByte();
        var sourceId = new Uuid(reader.ReadByteArraySlow(16));
        var transactionId = reader.ReadInt64LittleEndian();
        var gtid = new Gtid(sourceId, transactionId);

        return new GtidEvent(gtid, flags);
    }
}