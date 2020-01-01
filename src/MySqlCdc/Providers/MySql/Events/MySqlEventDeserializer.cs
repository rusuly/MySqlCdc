using MySqlCdc.Constants;
using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql
{
    public class MySqlEventDeserializer : EventDeserializer
    {
        public MySqlEventDeserializer()
        {
            EventParsers[EventType.MYSQL_GTID_EVENT] = new MySqlGtidEventParser();
        }
    }
}
