using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public interface IEventParser
    {
        IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader);
    }
}
