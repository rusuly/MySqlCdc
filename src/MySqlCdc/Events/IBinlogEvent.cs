using MySqlCdc.Protocol;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents a binlog event.
    /// </summary>
    public interface IBinlogEvent : IPacket
    {
        /// <summary>
        /// Gets the event header
        /// </summary>
        EventHeader Header { get; }
    }
}
