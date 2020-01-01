using System.Collections;
using System.Collections.Generic;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents one or many updated rows in row based replication.
    /// Includes versions before and after update.
    /// <see cref="https://mariadb.com/kb/en/library/rows_event_v1/"/>
    /// </summary>
    public class UpdateRowsEvent : BinlogEvent
    {
        public long TableId { get; }
        public int Flags { get; }
        public int ColumnsNumber { get; }
        public BitArray ColumnsPresentBeforeUpdate { get; }
        public BitArray ColumnsPresentAfterUpdate { get; }
        public IReadOnlyList<UpdateColumnData> Rows { get; }

        public UpdateRowsEvent(
            EventHeader header,
            long tableId,
            int flags,
            int columnsNumber,
            BitArray columnsPresentBeforeUpdate,
            BitArray columnsPresentAfterUpdate,
            IReadOnlyList<UpdateColumnData> rows)
            : base(header)
        {
            TableId = tableId;
            Flags = flags;
            ColumnsNumber = columnsNumber;
            ColumnsPresentBeforeUpdate = columnsPresentBeforeUpdate;
            ColumnsPresentAfterUpdate = columnsPresentAfterUpdate;
            Rows = rows;
        }
    }
}
