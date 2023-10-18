namespace MySqlCdc.Events;

/// <summary>
/// Represents sql statement in binary log.
/// <a href="https://mariadb.com/kb/en/library/query_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="QueryEvent"/>.
/// </remarks>
public record QueryEvent(
    long ThreadId,
    long Duration,
    int ErrorCode,
    byte[] StatusVariables,
    string DatabaseName,
    string SqlStatement) : IBinlogEvent
{
    /// <summary>
    /// Gets id of the thread that issued the statement.
    /// </summary>
    public long ThreadId { get; } = ThreadId;

    /// <summary>
    /// Gets the execution time of the statement in seconds.
    /// </summary>
    public long Duration { get; } = Duration;

    /// <summary>
    /// Gets the error code of the executed statement.
    /// </summary>
    public int ErrorCode { get; } = ErrorCode;

    /// <summary>
    /// Gets status variables.
    /// </summary>
    public byte[] StatusVariables { get; } = StatusVariables;

    /// <summary>
    /// Gets the default database name.
    /// </summary>
    public string DatabaseName { get; } = DatabaseName;

    /// <summary>
    /// Gets the SQL statement.
    /// </summary>
    public string SqlStatement { get; } = SqlStatement;
}