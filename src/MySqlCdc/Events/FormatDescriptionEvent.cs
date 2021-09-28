using MySqlCdc.Constants;

namespace MySqlCdc.Events;

/// <summary>
/// Written as the first event in binlog file or when replication is started.
/// See <a href="https://mariadb.com/kb/en/library/format_description_event/">MariaDB docs</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/format-description-event.html">MySQL docs</a>
/// See <a href="https://mariadb.com/kb/en/library/5-slave-registration/#events-transmission-after-com_binlog_dump">start events flow</a>
/// </summary>
public class FormatDescriptionEvent : BinlogEvent
{
    /// <summary>
    /// Gets binary log format version. This should always be 4.
    /// </summary>
    public int BinlogVersion { get; }

    /// <summary>
    /// Gets MariaDB/MySQL server version name.
    /// </summary>
    public string ServerVersion { get; }

    /// <summary>
    /// Gets checksum algorithm type.
    /// </summary>
    public ChecksumType ChecksumType { get; }

    /// <summary>
    /// Creates a new <see cref="FormatDescriptionEvent"/>.
    /// </summary>
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