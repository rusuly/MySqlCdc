namespace MySqlCdc.Events;

/// <summary>
/// Represents other binlog events.
/// </summary>
/// <remarks>
/// Creates a new <see cref="UnknownEvent"/>.
/// </remarks>
public record UnknownEvent : IBinlogEvent;