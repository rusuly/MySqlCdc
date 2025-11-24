namespace MySqlCdc.Events;

/// <summary>
/// Represents one or many inserted rows in row based replication.
/// <a href="https://mariadb.com/kb/en/library/rows_event_v1/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="WriteRowsEvent"/>.
/// </remarks>
public record WriteRowsEvent(
    long TableId,
    int Flags,
    int ColumnsNumber,
    bool[] ColumnsPresent,
    IReadOnlyList<RowData> Rows) : IBinlogRowsEvent
{
    /// <summary>
    /// Gets id of the table where rows were inserted
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
    /// Gets bitmap of columns present in row event. See binlog_row_image parameter.
    /// </summary>
    public bool[] ColumnsPresent { get; } = ColumnsPresent;

    /// <summary>
    /// Gets inserted rows
    /// </summary>
    public IReadOnlyList<RowData> Rows { get; } = Rows;
}