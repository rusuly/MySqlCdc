namespace MySqlCdc.Events;

/// <summary>
/// Represents a transaction commit event.
/// <a href="https://mariadb.com/kb/en/library/xid_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="XidEvent"/>.
/// </remarks>
public record XidEvent(long Xid) : IBinlogEvent
{
    /// <summary>
    /// Gets the XID transaction number
    /// </summary>
    public long Xid { get; } = Xid;
}