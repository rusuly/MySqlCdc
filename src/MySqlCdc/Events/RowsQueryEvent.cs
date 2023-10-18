namespace MySqlCdc.Events;

/// <summary>
/// Represents query that caused row events.
/// See <a href="https://dev.mysql.com/doc/internals/en/rows-query-event.html">MySQL docs</a>
/// See <a href="https://mariadb.com/kb/en/annotate_rows_event/">MariaDB docs</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="RowsQueryEvent"/>.
/// </remarks>
public record RowsQueryEvent(string Query) : IBinlogEvent
{
    /// <summary>
    /// Gets SQL statement
    /// </summary>
    public string Query { get; } = Query;
}