using System.Buffers;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class RotateEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var binlogPosition = reader.ReadLong(8);
            var binlogFilename = reader.ReadStringToEndOfFile();

            return new RotateEvent(header, binlogFilename, binlogPosition);
        }
    }
}
