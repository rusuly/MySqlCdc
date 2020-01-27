using System.Collections;
using System.Collections.Generic;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents one or many inserted rows in row based replication.
    /// <a href="https://mariadb.com/kb/en/library/rows_event_v1/">See more</a>
    /// </summary>
    public class WriteRowsEvent : BinlogEvent
    {
        /// <summary>
        /// Gets id of the table where rows were inserted
        /// </summary>
        public long TableId { get; }

        /// <summary>
        /// Gets <a href="https://mariadb.com/kb/en/rows_event_v1/#flags">flags</a>
        /// </summary>
        public int Flags { get; }

        /// <summary>
        /// Gets number of columns in the table
        /// </summary>
        public int ColumnsNumber { get; }

        /// <summary>
        /// Gets bitmap of columns present in row event. See binlog_row_image parameter.
        /// </summary>
        public BitArray ColumnsPresent { get; }

        /// <summary>
        /// Gets inserted rows
        /// </summary>
        public IReadOnlyList<ColumnData> Rows { get; }

        /// <summary>
        /// Creates a new <see cref="WriteRowsEvent"/>.
        /// </summary>
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
