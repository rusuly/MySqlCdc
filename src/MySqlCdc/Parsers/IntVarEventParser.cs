using System.Buffers;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class IntVarEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var type = reader.ReadInt(1);
            var value = reader.ReadLong(8);

            return new IntVarEvent(header, type, value);
        }
    }
}
