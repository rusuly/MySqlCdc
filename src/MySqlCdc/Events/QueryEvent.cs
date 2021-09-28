namespace MySqlCdc.Events;

/// <summary>
/// Represents sql statement in binary log.
/// <a href="https://mariadb.com/kb/en/library/query_event/">See more</a>
/// </summary>
public class QueryEvent : BinlogEvent
{
    /// <summary>
    /// Gets id of the thread that issued the statement.
    /// </summary>
    public long ThreadId { get; }

    /// <summary>
    /// Gets the execution time of the statement in seconds.
    /// </summary>
    public long Duration { get; }

    /// <summary>
    /// Gets the error code of the executed statement.
    /// </summary>
    public int ErrorCode { get; }

    /// <summary>
    /// Gets status variables.
    /// </summary>
    public byte[] StatusVariables { get; }

    /// <summary>
    /// Gets the default database name.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Gets the SQL statement.
    /// </summary>
    public string SqlStatement { get; }

    /// <summary>
    /// Creates a new <see cref="QueryEvent"/>.
    /// </summary>
    public QueryEvent(
        EventHeader header,
        long threadId,
        long duration,
        int errorCode,
        byte[] statusVariables,
        string databaseName,
        string sqlStatement) : base(header)
    {
        ThreadId = threadId;
        Duration = duration;
        ErrorCode = errorCode;
        StatusVariables = statusVariables;
        DatabaseName = databaseName;
        SqlStatement = sqlStatement;
    }
}