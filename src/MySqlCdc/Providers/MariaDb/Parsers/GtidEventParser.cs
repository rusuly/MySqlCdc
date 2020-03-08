using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MariaDb
{
    /// <summary>
    /// Parses <see cref="GtidEvent"/> events in MariaDB 10.0.2+.
    /// </summary>
    public class GtidEventParser : IEventParser
    {
        /// <summary>
        /// Parses <see cref="GtidEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            long sequence = reader.ReadInt64LittleEndian();
            long domainId = (uint)reader.ReadInt32LittleEndian();
            var gtid = new Gtid(domainId, header.ServerId, sequence);

            byte flags = reader.ReadByte();
            return new GtidEvent(header, gtid, flags);
        }
    }
}
