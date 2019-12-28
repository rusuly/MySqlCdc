using MySql.Cdc.Constants;
using MySql.Cdc.Events;

namespace MySql.Cdc.Providers.MySql
{
    public class MySqlEventDeserializer : EventDeserializer
    {
        public MySqlEventDeserializer()
        {
            EventParsers[EventType.MYSQL_GTID_EVENT] = new MySqlGtidEventParser();
        }
    }
}
