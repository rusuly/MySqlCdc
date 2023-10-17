using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Events;

/// <summary>
/// Used to record the gtid_executed of previous binlog files.
/// </summary>
public class PreviousGtidsEvent : IBinlogEvent
{
    /// <summary>
    /// Gets GtidSet of previous files.
    /// </summary>
    public GtidSet GtidSet { get; }

    /// <summary>
    /// Creates a new <see cref="PreviousGtidsEvent"/>.
    /// </summary>
    public PreviousGtidsEvent(GtidSet gtidSet)
    {
        GtidSet = gtidSet;
    }
}