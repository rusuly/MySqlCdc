namespace MySqlCdc.Events
{
    /// <summary>
    /// Marks start of a new event group(transaction).
    /// <a href="https://mariadb.com/kb/en/gtid_event/">See more</a>
    /// </summary>
    public class GtidEvent : BinlogEvent
    {
        /// <summary>
        /// Gets Global Transaction ID of the event group.
        /// </summary>
        public string Gtid { get; }

        /// <summary>
        /// Gets flags.
        /// </summary>
        public int Flags { get; }

        /// <summary>
        /// Creates a new <see cref="GtidEvent"/>.
        /// </summary>
        public GtidEvent(EventHeader header, string gtid, int flags) : base(header)
        {
            Gtid = gtid;
            Flags = flags;
        }
    }
}
