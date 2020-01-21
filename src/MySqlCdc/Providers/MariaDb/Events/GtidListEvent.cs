using System.Collections.Generic;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Shows current replication state with list of last gtid for each replication domain.
    /// <a href="https://mariadb.com/kb/en/gtid_list_event/">See more</a>
    /// </summary>
    public class GtidListEvent : BinlogEvent
    {
        public IReadOnlyList<string> GtidList { get; }

        public GtidListEvent(EventHeader header, List<string> gtidList) : base(header)
        {
            GtidList = gtidList;
        }
    }
}
