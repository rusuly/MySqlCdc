using MySqlCdc.Protocol;

namespace MySqlCdc.Events;

internal record HeaderWithEvent(EventHeader Header, IBinlogEvent Event) : IPacket;