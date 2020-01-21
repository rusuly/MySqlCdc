namespace MySqlCdc.Events
{
    /// <summary>
    /// Generated for a commit of a transaction.
    /// <a href="https://mariadb.com/kb/en/library/xid_event/">See more</a>
    /// </summary>
    public class XidEvent : BinlogEvent
    {
        public long Xid { get; }

        public XidEvent(EventHeader header, long xid) : base(header)
        {
            Xid = xid;
        }
    }
}
