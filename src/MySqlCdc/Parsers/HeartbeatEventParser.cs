using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class HeartbeatEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var binlogFilename = reader.ReadStringToEndOfFile();

            return new HeartbeatEvent(header, binlogFilename);
        }
    }
}
