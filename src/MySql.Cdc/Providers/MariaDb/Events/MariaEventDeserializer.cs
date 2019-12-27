using MySql.Cdc.Constants;
using MySql.Cdc.Events;

namespace MySql.Cdc.Providers.MariaDb
{
    public class MariaEventDeserializer : EventDeserializer
    {
        public MariaEventDeserializer()
        {
            EventParsers[EventType.MARIADB_GTID_EVENT] = new MariaGtidEventParser();
            EventParsers[EventType.MARIADB_GTID_LIST_EVENT] = new MariaGtidListEventParser();
        }
    }
}
