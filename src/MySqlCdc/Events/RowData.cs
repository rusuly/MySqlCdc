namespace MySqlCdc.Events;

/// <summary>
/// Represents an inserted or deleted row in row based replication.
/// </summary>
/// <remarks>
/// Creates a new <see cref="RowData"/>.
/// </remarks>
public record RowData(IReadOnlyList<object?> Cells)
{
    /// <summary>
    /// Column values of the changed row.
    /// </summary>
    public IReadOnlyList<object?> Cells { get; } = Cells;
}

/// <summary>
/// Represents an updated row in row based replication.
/// </summary>
/// <remarks>
/// Creates a new <see cref="UpdateRowData"/>.
/// </remarks>
public record UpdateRowData(RowData BeforeUpdate, RowData AfterUpdate)
{
    /// <summary>
    /// Row state before it was updated.
    /// </summary>
    public RowData BeforeUpdate { get; } = BeforeUpdate;

    /// <summary>
    /// Actual row state after update.
    /// </summary>
    public RowData AfterUpdate { get; } = AfterUpdate;
}