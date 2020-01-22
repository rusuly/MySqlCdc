using MySqlCdc.Constants;
using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql
{
    public class MySqlEventDeserializer : EventDeserializer
    {
        public MySqlEventDeserializer()
        {
            EventParsers[EventType.MYSQL_GTID_EVENT] = new MySqlGtidEventParser();
            EventParsers[EventType.MYSQL_ROWS_QUERY_EVENT] = new RowsQueryEventParser();
        }
    }
}
