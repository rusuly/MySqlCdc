using System.Collections.Generic;

namespace MySqlCdc.Events;

/// <summary>
/// Represents an inserted or deleted row in row based replication.
/// </summary>
public class RowData
{
    /// <summary>
    /// Column values of the changed row.
    /// </summary>
    public IReadOnlyList<object?> Cells { get; }

    /// <summary>
    /// Creates a new <see cref="RowData"/>.
    /// </summary>
    public RowData(IReadOnlyList<object?> cells)
    {
        Cells = cells;
    }
}

/// <summary>
/// Represents an updated row in row based replication.
/// </summary>
public class UpdateRowData
{
    /// <summary>
    /// Row state before it was updated.
    /// </summary>
    public RowData BeforeUpdate { get; }

    /// <summary>
    /// Actual row state after update.
    /// </summary>
    public RowData AfterUpdate { get; }

    /// <summary>
    /// Creates a new <see cref="UpdateRowData"/>.
    /// </summary>
    public UpdateRowData(RowData beforeUpdate, RowData afterUpdate)
    {
        BeforeUpdate = beforeUpdate;
        AfterUpdate = afterUpdate;
    }
}