using MySqlCdc.Constants;
using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// MySQL binlog events deserializer.
    /// </summary>
    public class MySqlEventDeserializer : EventDeserializer
    {
        /// <summary>
        /// Creates a new <see cref="MySqlEventDeserializer"/>.
        /// </summary>
        public MySqlEventDeserializer()
        {
            EventParsers[EventType.MYSQL_GTID_EVENT] = new GtidEventParser();
            EventParsers[EventType.MYSQL_ROWS_QUERY_EVENT] = new RowsQueryEventParser();
            EventParsers[EventType.MYSQL_PREVIOUS_GTIDS_EVENT] = new PreviousGtidsEventParser();
        }
    }
}
