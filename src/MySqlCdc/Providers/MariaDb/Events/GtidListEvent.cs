using MySqlCdc.Providers.MariaDb;

namespace MySqlCdc.Events;

/// <summary>
/// Shows current replication state with list of last gtid for each replication domain.
/// <a href="https://mariadb.com/kb/en/gtid_list_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="GtidListEvent"/>.
/// </remarks>
public record GtidListEvent(GtidList GtidList) : IBinlogEvent
{
    /// <summary>
    /// Gets a list of Gtid that represents current replication state
    /// </summary>
    public GtidList GtidList { get; } = GtidList;
}