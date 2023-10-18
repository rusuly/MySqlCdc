namespace MySqlCdc.Events;

/// <summary>
/// Represents a Global Transaction Identifier.
/// </summary>
public interface IGtid { }

/// <summary>
/// Represents replication Gtid position.
/// </summary>
public interface IGtidState
{
    /// <summary>
    /// Adds a gtid value to the state.
    /// </summary>
    bool AddGtid(IGtid gtid);
}

/// <summary>
/// Marks start of a new event group(transaction).
/// <a href="https://mariadb.com/kb/en/gtid_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="GtidEvent"/>.
/// </remarks>
public record GtidEvent(IGtid Gtid, byte Flags) : IBinlogEvent
{
    /// <summary>
    /// Gets Global Transaction ID of the event group.
    /// </summary>
    public IGtid Gtid { get; } = Gtid;

    /// <summary>
    /// Gets flags.
    /// </summary>
    public byte Flags { get; } = Flags;
}