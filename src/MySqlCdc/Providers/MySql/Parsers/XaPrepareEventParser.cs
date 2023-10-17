using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MySql;

/// <summary>
/// Parses <see cref="XaPrepareEvent"/> events in MySQL 5.6+.
/// </summary>
public class XaPrepareEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="XaPrepareEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        bool onePhase = reader.ReadByte() != 0x00;
        int formatId = (int)reader.ReadUInt32LittleEndian();
        int gtridLength = (int)reader.ReadUInt32LittleEndian();
        int bqualLength = (int)reader.ReadUInt32LittleEndian();
        string gtrid = reader.ReadString(gtridLength);
        string bqual = reader.ReadString(bqualLength);

        return new XaPrepareEvent(onePhase, formatId, gtrid, bqual);
    }
}