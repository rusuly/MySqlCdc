using System.Collections.Generic;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Shows current replication state with list of last gtid for each replication domain.
    /// <see cref="https://mariadb.com/kb/en/gtid_list_event/"/>
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
