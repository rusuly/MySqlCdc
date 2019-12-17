using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    public class UnknownEvent : BinlogEvent
    {
        public UnknownEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);
        }
    }
}
