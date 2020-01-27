using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Base interface for binlog event parsers.
    /// </summary>
    public interface IEventParser
    {
        /// <summary>
        /// Parses a binlog event from the buffer.
        /// </summary>
        IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader);
    }
}
