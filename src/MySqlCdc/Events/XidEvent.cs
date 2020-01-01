namespace MySqlCdc.Events
{
    /// <summary>
    /// Generated for a commit of a transaction.
    /// <see cref="https://mariadb.com/kb/en/library/xid_event/"/>
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
