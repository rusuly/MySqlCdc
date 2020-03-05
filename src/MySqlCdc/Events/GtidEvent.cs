namespace MySqlCdc.Events
{
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
    public class GtidEvent : BinlogEvent
    {
        /// <summary>
        /// Gets Global Transaction ID of the event group.
        /// </summary>
        public IGtid Gtid { get; }

        /// <summary>
        /// Gets flags.
        /// </summary>
        public int Flags { get; }

        /// <summary>
        /// Creates a new <see cref="GtidEvent"/>.
        /// </summary>
        public GtidEvent(EventHeader header, IGtid gtid, int flags) : base(header)
        {
            Gtid = gtid;
            Flags = flags;
        }
    }
}
