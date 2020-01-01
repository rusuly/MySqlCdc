using System.Collections;
using System.Collections.Generic;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents one or many inserted rows in row based replication.
    /// <see cref="https://mariadb.com/kb/en/library/rows_event_v1/"/>
    /// </summary>
    public class WriteRowsEvent : BinlogEvent
    {
        public long TableId { get; }
        public int Flags { get; }
        public int ColumnsNumber { get; }
        public BitArray ColumnsPresent { get; }
        public IReadOnlyList<ColumnData> Rows { get; }

        public WriteRowsEvent(
            EventHeader header,
            long tableId,
            int flags,
            int columnsNumber,
            BitArray columnsPresent,
            IReadOnlyList<ColumnData> rows)
            : base(header)
        {
            TableId = tableId;
            Flags = flags;
            ColumnsNumber = columnsNumber;
            ColumnsPresent = columnsPresent;
            Rows = rows;
        }
    }
}
