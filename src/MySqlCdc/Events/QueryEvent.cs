namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents sql statement in binary log.
    /// <a href="https://mariadb.com/kb/en/library/query_event/">See more</a>
    /// </summary>
    public class QueryEvent : BinlogEvent
    {
        public long ThreadId { get; }
        public long Duration { get; }
        public int ErrorCode { get; }
        public byte[] StatusVariables { get; }
        public string DatabaseName { get; }
        public string SqlStatement { get; }

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
}
