using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Represents an event from replicated binlog event stream.
    /// </summary>
    public interface IBinlogEvent : IPacket
    {
        EventHeader Header { get; }
    }
}
