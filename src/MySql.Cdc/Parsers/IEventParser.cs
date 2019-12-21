using System.Buffers;
using MySql.Cdc.Events;

namespace MySql.Cdc.Parsers
{
    public interface IEventParser
    {
        IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer);
    }
}
