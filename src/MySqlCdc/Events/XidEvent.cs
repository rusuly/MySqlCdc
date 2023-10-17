namespace MySqlCdc.Events;

/// <summary>
/// Represents a transaction commit event.
/// <a href="https://mariadb.com/kb/en/library/xid_event/">See more</a>
/// </summary>
public class XidEvent : IBinlogEvent
{
    /// <summary>
    /// Gets the XID transaction number
    /// </summary>
    public long Xid { get; }

    /// <summary>
    /// Creates a new <see cref="XidEvent"/>.
    /// </summary>
    public XidEvent(long xid)
    {
        Xid = xid;
    }
}