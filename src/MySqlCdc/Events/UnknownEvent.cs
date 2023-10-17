namespace MySqlCdc.Events;

/// <summary>
/// Represents other binlog events.
/// </summary>
public class UnknownEvent : IBinlogEvent
{
    /// <summary>
    /// Creates a new <see cref="UnknownEvent"/>.
    /// </summary>
    public UnknownEvent()
    {
    }
}