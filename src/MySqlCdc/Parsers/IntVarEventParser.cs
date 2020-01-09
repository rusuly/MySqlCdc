using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class IntVarEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var type = reader.ReadInt(1);
            var value = reader.ReadLong(8);

            return new IntVarEvent(header, type, value);
        }
    }
}
