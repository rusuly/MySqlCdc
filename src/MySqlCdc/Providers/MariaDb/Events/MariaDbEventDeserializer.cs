using MySqlCdc.Constants;
using MySqlCdc.Events;

namespace MySqlCdc.Providers.MariaDb;

/// <summary>
/// MariaDB binlog events deserializer.
/// </summary>
public class MariaDbEventDeserializer : EventDeserializer
{
    /// <summary>
    /// Creates a new <see cref="MariaDbEventDeserializer"/>.
    /// </summary>
    public MariaDbEventDeserializer()
    {
        EventParsers[EventType.MARIADB_GTID_EVENT] = new GtidEventParser();
        EventParsers[EventType.MARIADB_GTID_LIST_EVENT] = new GtidListEventParser();
        EventParsers[EventType.MARIADB_ANNOTATE_ROWS_EVENT] = new AnnotateRowsEventParser();
    }
}