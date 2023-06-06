namespace MySqlCdc.Events;

/// <summary>
/// Represents one or many updated rows in row based replication.
/// Includes versions before and after update.
/// <a href="https://mariadb.com/kb/en/library/rows_event_v1/">See more</a>
/// </summary>
public class UpdateRowsEvent : BinlogEvent
{
    /// <summary>
    /// Gets id of the table where rows were updated
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
    /// Gets bitmap of columns present in row event before update. See binlog_row_image parameter.
    /// </summary>
    public bool[] ColumnsPresentBeforeUpdate { get; }

    /// <summary>
    /// Gets bitmap of columns present in row event after update. See binlog_row_image parameter.
    /// </summary>
    public bool[] ColumnsPresentAfterUpdate { get; }

    /// <summary>
    /// Gets updated rows
    /// </summary>
    public IReadOnlyList<UpdateRowData> Rows { get; }

    /// <summary>
    /// Creates a new <see cref="UpdateRowsEvent"/>.
    /// </summary>
    public UpdateRowsEvent(
        EventHeader header,
        long tableId,
        int flags,
        int columnsNumber,
        bool[] columnsPresentBeforeUpdate,
        bool[] columnsPresentAfterUpdate,
        IReadOnlyList<UpdateRowData> rows)
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