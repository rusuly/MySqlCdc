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
        EventParsers[EventType.MariaDbGtidEvent] = new GtidEventParser();
        EventParsers[EventType.MariaDbGtidListEvent] = new GtidListEventParser();
        EventParsers[EventType.MariaDbAnnotateRowsEvent] = new AnnotateRowsEventParser();
    }
}