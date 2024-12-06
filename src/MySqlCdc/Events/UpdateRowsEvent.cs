namespace MySqlCdc.Events;

/// <summary>
/// Represents one or many updated rows in row based replication.
/// Includes versions before and after update.
/// <a href="https://mariadb.com/kb/en/library/rows_event_v1/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="UpdateRowsEvent"/>.
/// </remarks>
public record UpdateRowsEvent(
    long TableId,
    int Flags,
    int ColumnsNumber,
    bool[] ColumnsPresentBeforeUpdate,
    bool[] ColumnsPresentAfterUpdate,
    IReadOnlyList<UpdateRowData> Rows) : IBinlogRowsEvent
{
    /// <summary>
    /// Gets id of the table where rows were updated
    /// </summary>
    public long TableId { get; } = TableId;

    /// <summary>
    /// Gets <a href="https://mariadb.com/kb/en/rows_event_v1/#flags">flags</a>
    /// </summary>
    public int Flags { get; } = Flags;

    /// <summary>
    /// Gets number of columns in the table
    /// </summary>
    public int ColumnsNumber { get; } = ColumnsNumber;

    /// <summary>
    /// Gets bitmap of columns present in row event before update. See binlog_row_image parameter.
    /// </summary>
    public bool[] ColumnsPresentBeforeUpdate { get; } = ColumnsPresentBeforeUpdate;

    /// <summary>
    /// Gets bitmap of columns present in row event after update. See binlog_row_image parameter.
    /// </summary>
    public bool[] ColumnsPresentAfterUpdate { get; } = ColumnsPresentAfterUpdate;

    /// <summary>
    /// Gets updated rows
    /// </summary>
    public IReadOnlyList<UpdateRowData> Rows { get; } = Rows;
}