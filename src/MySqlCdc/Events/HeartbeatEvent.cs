namespace MySqlCdc.Events
{
    /// <summary>
    /// The event is sent from master to the client for keep alive feature.
    /// <see cref="https://mariadb.com/kb/en/library/heartbeat_log_event/"/>
    /// </summary>
    public class HeartbeatEvent : BinlogEvent
    {
        public string BinlogFilename { get; }

        public HeartbeatEvent(EventHeader header, string binlogFilename) : base(header)
        {
            BinlogFilename = binlogFilename;
        }
    }
}
