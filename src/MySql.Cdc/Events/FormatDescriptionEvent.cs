using MySql.Cdc.Constants;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Written as the first event in binlog file or when replication is started.
    /// <see cref="https://mariadb.com/kb/en/library/format_description_event/"/>
    /// <see cref="https://mariadb.com/kb/en/library/5-slave-registration/#events-transmission-after-com_binlog_dump"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/format-description-event.html"/>
    /// </summary>
    public class FormatDescriptionEvent : BinlogEvent
    {
        public int BinlogVersion { get; }
        public string ServerVersion { get; }
        public ChecksumType ChecksumType { get; }

        public FormatDescriptionEvent(
            EventHeader header,
            int binlogVersion,
            string serverVersion,
            ChecksumType checksumType) : base(header)
        {
            BinlogVersion = binlogVersion;
            ServerVersion = serverVersion;
            ChecksumType = checksumType;
        }
    }
}
