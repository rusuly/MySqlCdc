using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MySql
{
    public class RowsQueryEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            reader.Skip(1);
            var query = reader.ReadStringToEndOfFile();

            return new RowsQueryEvent(header, query);
        }
    }
}
