using System.Buffers;
using MySql.Cdc.Events;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Parsers
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
