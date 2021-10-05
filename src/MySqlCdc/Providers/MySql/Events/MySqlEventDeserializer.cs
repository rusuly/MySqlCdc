using MySqlCdc.Constants;
using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql;

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
        EventParsers[EventType.MySqlGtidEvent] = new GtidEventParser();
        EventParsers[EventType.MySqlRowsQueryEvent] = new RowsQueryEventParser();
        EventParsers[EventType.MySqlPreviousGtidsEvent] = new PreviousGtidsEventParser();
        EventParsers[EventType.MySqlXaPrepareEvent] = new XaPrepareEventParser();
    }
}