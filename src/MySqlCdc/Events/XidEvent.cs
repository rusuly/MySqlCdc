namespace MySqlCdc.Events;

/// <summary>
/// Represents a transaction commit event.
/// <a href="https://mariadb.com/kb/en/library/xid_event/">See more</a>
/// </summary>
public class XidEvent : BinlogEvent
{
    /// <summary>
    /// Gets the XID transaction number
    /// </summary>
    public long Xid { get; }

    /// <summary>
    /// Creates a new <see cref="XidEvent"/>.
    /// </summary>
    public XidEvent(EventHeader header, long xid) : base(header)
    {
        Xid = xid;
    }
}