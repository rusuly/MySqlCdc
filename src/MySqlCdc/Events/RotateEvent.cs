namespace MySqlCdc.Events;

/// <summary>
/// Last event in a binlog file which points to next binlog file.
/// Fake version is also returned when replication is started.
/// <a href="https://mariadb.com/kb/en/library/rotate_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="RotateEvent"/>.
/// </remarks>
public record RotateEvent(string BinlogFilename, long BinlogPosition) : IBinlogEvent
{
    /// <summary>
    /// Gets next binlog filename
    /// </summary>
    public string BinlogFilename { get; } = BinlogFilename;

    /// <summary>
    /// Gets next binlog position
    /// </summary>
    public long BinlogPosition { get; } = BinlogPosition;
}