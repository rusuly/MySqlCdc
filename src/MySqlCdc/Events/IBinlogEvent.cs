using MySqlCdc.Protocol;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents an event from replicated binlog event stream.
    /// </summary>
    public interface IBinlogEvent : IPacket
    {
        EventHeader Header { get; }
    }
}
