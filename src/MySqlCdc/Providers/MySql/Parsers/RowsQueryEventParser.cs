using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// Parses <see cref="RowsQueryEvent"/> events in MySQL 5.6+.
    /// </summary>
    public class RowsQueryEventParser : IEventParser
    {
        /// <summary>
        /// Parses <see cref="RowsQueryEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            reader.Advance(1);
            var query = reader.ReadStringToEndOfFile();

            return new RowsQueryEvent(header, query);
        }
    }
}
