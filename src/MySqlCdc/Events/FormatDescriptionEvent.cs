using MySqlCdc.Constants;

namespace MySqlCdc.Events;

/// <summary>
/// Written as the first event in binlog file or when replication is started.
/// See <a href="https://mariadb.com/kb/en/library/format_description_event/">MariaDB docs</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/format-description-event.html">MySQL docs</a>
/// See <a href="https://mariadb.com/kb/en/library/5-slave-registration/#events-transmission-after-com_binlog_dump">start events flow</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="FormatDescriptionEvent"/>.
/// </remarks>
public record FormatDescriptionEvent(
    int BinlogVersion,
    string ServerVersion,
    ChecksumType ChecksumType) : IBinlogEvent
{
    /// <summary>
    /// Gets binary log format version. This should always be 4.
    /// </summary>
    public int BinlogVersion { get; } = BinlogVersion;

    /// <summary>
    /// Gets MariaDB/MySQL server version name.
    /// </summary>
    public string ServerVersion { get; } = ServerVersion;

    /// <summary>
    /// Gets checksum algorithm type.
    /// </summary>
    public ChecksumType ChecksumType { get; } = ChecksumType;
}