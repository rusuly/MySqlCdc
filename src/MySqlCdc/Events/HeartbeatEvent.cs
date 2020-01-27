namespace MySqlCdc.Events
{
    /// <summary>
    /// The event is sent from master to the client for keep alive feature.
    /// <a href="https://mariadb.com/kb/en/library/heartbeat_log_event/">See more</a>
    /// </summary>
    public class HeartbeatEvent : BinlogEvent
    {
        /// <summary>
        /// Gets current master binlog filename
        /// </summary>
        public string BinlogFilename { get; }

        /// <summary>
        /// Creates a new <see cref="HeartbeatEvent"/>.
        /// </summary>
        public HeartbeatEvent(EventHeader header, string binlogFilename) : base(header)
        {
            BinlogFilename = binlogFilename;
        }
    }
}
