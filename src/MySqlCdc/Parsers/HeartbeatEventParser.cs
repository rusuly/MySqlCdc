using System.Buffers;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class HeartbeatEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var binlogFilename = reader.ReadStringToEndOfFile();

            return new HeartbeatEvent(header, binlogFilename);
        }
    }
}
