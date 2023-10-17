using MySqlCdc.Protocol;

namespace MySqlCdc.Events;

internal class HeaderWithEvent : IPacket
{
    public EventHeader Header { get; }
    public IBinlogEvent Event { get; }

    public HeaderWithEvent(EventHeader header, IBinlogEvent @event)
    {
        Header = header;
        Event = @event;
    }
}