namespace MySqlCdc.Events
{
    /// <summary>
    /// Last event in a binlog file which points to next binlog file.
    /// Fake version is also returned when replication is started.
    /// <a href="https://mariadb.com/kb/en/library/rotate_event/">See more</a>
    /// </summary>
    public class RotateEvent : BinlogEvent
    {
        /// <summary>
        /// Gets next binlog filename
        /// </summary>
        public string BinlogFilename { get; }

        /// <summary>
        /// Gets next binlog position
        /// </summary>
        public long BinlogPosition { get; }

        /// <summary>
        /// Creates a new <see cref="RotateEvent"/>.
        /// </summary>
        public RotateEvent(EventHeader header, string binlogFilename, long binlogPosition)
            : base(header)
        {
            BinlogFilename = binlogFilename;
            BinlogPosition = binlogPosition;
        }
    }
}
