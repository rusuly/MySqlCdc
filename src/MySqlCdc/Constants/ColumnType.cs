namespace MySqlCdc.Constants
{
    /// <summary>
    /// MySql column types.
    /// See <a href="https://mariadb.com/kb/en/library/resultset/#column-definition-packet">MariaDB docs</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/com-query-response.html#column-type">MySQL docs</a>
    /// </summary>
    public enum ColumnType
    {
        /// <summary>
        /// DECIMAL
        /// </summary>
        DECIMAL = 0,
        
        /// <summary>
        /// TINY
        /// </summary>
        TINY,
        
        /// <summary>
        /// SHORT
        /// </summary>
        SHORT,
        
        /// <summary>
        /// LONG
        /// </summary>
        LONG,
        
        /// <summary>
        /// FLOAT
        /// </summary>
        FLOAT,
        
        /// <summary>
        /// DOUBLE
        /// </summary>
        DOUBLE,
        
        /// <summary>
        /// NULL
        /// </summary>
        NULL,
        
        /// <summary>
        /// TIMESTAMP
        /// </summary>
        TIMESTAMP,
        
        /// <summary>
        /// LONGLONG
        /// </summary>
        LONGLONG,
        
        /// <summary>
        /// INT24
        /// </summary>
        INT24,
        
        /// <summary>
        /// DATE
        /// </summary>
        DATE,
        
        /// <summary>
        /// TIME
        /// </summary>
        TIME,
        
        /// <summary>
        /// DATETIME
        /// </summary>
        DATETIME,
        
        /// <summary>
        /// YEAR
        /// </summary>
        YEAR,
        
        /// <summary>
        /// NEWDATE
        /// </summary>
        NEWDATE,
        
        /// <summary>
        /// VARCHAR
        /// </summary>
        VARCHAR,
        
        /// <summary>
        /// BIT
        /// </summary>
        BIT,
        
        /// <summary>
        /// TIMESTAMP2
        /// </summary>
        TIMESTAMP2,
        
        /// <summary>
        /// DATETIME2
        /// </summary>
        DATETIME2,
        
        /// <summary>
        /// TIME2
        /// </summary>
        TIME2,

        /// <summary>
        /// JSON is MySQL 5.7.8+ type. Not supported in MariaDB.
        /// </summary>
        JSON = 245,
        
        /// <summary>
        /// NEWDECIMAL
        /// </summary>
        NEWDECIMAL,
        
        /// <summary>
        /// ENUM
        /// </summary>
        ENUM,
        
        /// <summary>
        /// SET
        /// </summary>
        SET,
        
        /// <summary>
        /// TINY_BLOB
        /// </summary>
        TINY_BLOB,
        
        /// <summary>
        /// MEDIUM_BLOB
        /// </summary>
        MEDIUM_BLOB,
        
        /// <summary>
        /// LONG_BLOB
        /// </summary>
        LONG_BLOB,
        
        /// <summary>
        /// BLOB
        /// </summary>
        BLOB,
        
        /// <summary>
        /// VAR_STRING
        /// </summary>
        VAR_STRING,
        
        /// <summary>
        /// STRING
        /// </summary>
        STRING,
        
        /// <summary>
        /// GEOMETRY
        /// </summary>
        GEOMETRY
    }
}
