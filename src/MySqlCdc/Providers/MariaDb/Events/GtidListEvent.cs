using System.Collections.Generic;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Shows current replication state with list of last gtid for each replication domain.
    /// <a href="https://mariadb.com/kb/en/gtid_list_event/">See more</a>
    /// </summary>
    public class GtidListEvent : BinlogEvent
    {
        /// <summary>
        /// Gets a list of Gtid that represents current replication state
        /// </summary>
        public IReadOnlyList<string> GtidList { get; }

        /// <summary>
        /// Creates a new <see cref="GtidListEvent"/>.
        /// </summary>
        public GtidListEvent(EventHeader header, List<string> gtidList) : base(header)
        {
            GtidList = gtidList;
        }
    }
}
