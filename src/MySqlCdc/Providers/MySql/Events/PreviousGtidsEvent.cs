using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Events;

/// <summary>
/// Used to record the gtid_executed of previous binlog files.
/// </summary>
/// <remarks>
/// Creates a new <see cref="PreviousGtidsEvent"/>.
/// </remarks>
public record PreviousGtidsEvent(GtidSet GtidSet) : IBinlogEvent
{
    /// <summary>
    /// Gets GtidSet of previous files.
    /// </summary>
    public GtidSet GtidSet { get; } = GtidSet;
}