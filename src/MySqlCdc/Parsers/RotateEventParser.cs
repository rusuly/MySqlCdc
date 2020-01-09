using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class RotateEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var binlogPosition = reader.ReadLong(8);
            var binlogFilename = reader.ReadStringToEndOfFile();

            return new RotateEvent(header, binlogFilename, binlogPosition);
        }
    }
}
