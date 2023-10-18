namespace MySqlCdc.Events;

/// <summary>
/// The event is sent from master to the client for keep alive feature.
/// <a href="https://mariadb.com/kb/en/library/heartbeat_log_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="HeartbeatEvent"/>.
/// </remarks>
public record HeartbeatEvent(string BinlogFilename) : IBinlogEvent
{
    /// <summary>
    /// Gets current master binlog filename
    /// </summary>
    public string BinlogFilename { get; } = BinlogFilename;
}