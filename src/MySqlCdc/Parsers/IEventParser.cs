using System.Buffers;
using MySqlCdc.Events;

namespace MySqlCdc.Parsers
{
    public interface IEventParser
    {
        IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer);
    }
}
