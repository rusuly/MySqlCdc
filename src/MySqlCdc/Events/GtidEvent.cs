namespace MySqlCdc.Events
{
    /// <summary>
    /// Marks start of a new event group(transaction).
    /// <see cref="https://mariadb.com/kb/en/gtid_event/"/>
    /// </summary>
    public class GtidEvent : BinlogEvent
    {
        public string Gtid { get; }
        public int Flags { get; }

        public GtidEvent(EventHeader header, string gtid, int flags) : base(header)
        {
            Gtid = gtid;
            Flags = flags;
        }
    }
}
