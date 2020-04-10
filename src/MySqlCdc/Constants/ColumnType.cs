namespace MySqlCdc.Constants
{
    /// <summary>
    /// MySql column types.
    /// See <a href="https://mariadb.com/kb/en/library/resultset/#column-definition-packet">MariaDB docs</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/com-query-response.html#column-type">MySQL docs</a>
    /// </summary>
    public enum ColumnType
    {
        DECIMAL = 0,
        TINY,
        SHORT,
        LONG,
        FLOAT,
        DOUBLE,
        NULL,
        TIMESTAMP,
        LONGLONG,
        INT24,
        DATE,
        TIME,
        DATETIME,
        YEAR,
        NEWDATE,
        VARCHAR,
        BIT,
        TIMESTAMP2,
        DATETIME2,
        TIME2,

        // JSON is MySQL 5.7.8+ type. Not supported in MariaDB.
        JSON = 245,
        NEWDECIMAL,
        ENUM,
        SET,
        TINY_BLOB,
        MEDIUM_BLOB,
        LONG_BLOB,
        BLOB,
        VAR_STRING,
        STRING,
        GEOMETRY
    }
}
