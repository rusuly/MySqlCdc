using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MariaDb
{
    public class AnnotateRowsEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var query = reader.ReadStringToEndOfFile();
            return new RowsQueryEvent(header, query);
        }
    }
}
