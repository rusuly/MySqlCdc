namespace MySqlCdc.Constants;

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
    Decimal = 0,
        
    /// <summary>
    /// TINY
    /// </summary>
    Tiny = 1,
        
    /// <summary>
    /// SHORT
    /// </summary>
    Short = 2,
        
    /// <summary>
    /// LONG
    /// </summary>
    Long = 3,
        
    /// <summary>
    /// FLOAT
    /// </summary>
    Float = 4,
        
    /// <summary>
    /// DOUBLE
    /// </summary>
    Double = 5,
        
    /// <summary>
    /// NULL
    /// </summary>
    Null = 6,
        
    /// <summary>
    /// TIMESTAMP
    /// </summary>
    Timestamp = 7,
        
    /// <summary>
    /// LONGLONG
    /// </summary>
    LongLong = 8,
        
    /// <summary>
    /// INT24
    /// </summary>
    Int24 = 9,
        
    /// <summary>
    /// DATE
    /// </summary>
    Date = 10,
        
    /// <summary>
    /// TIME
    /// </summary>
    Time = 11,
        
    /// <summary>
    /// DATETIME
    /// </summary>
    DateTime = 12,
        
    /// <summary>
    /// YEAR
    /// </summary>
    Year = 13,
        
    /// <summary>
    /// NEWDATE
    /// </summary>
    NewDate = 14,
        
    /// <summary>
    /// VARCHAR
    /// </summary>
    VarChar = 15,
        
    /// <summary>
    /// BIT
    /// </summary>
    Bit = 16,
        
    /// <summary>
    /// TIMESTAMP2
    /// </summary>
    TimeStamp2 = 17,
        
    /// <summary>
    /// DATETIME2
    /// </summary>
    DateTime2 = 18,
        
    /// <summary>
    /// TIME2
    /// </summary>
    Time2 = 19,

    /// <summary>
    /// JSON is MySQL 5.7.8+ type. Not supported in MariaDB.
    /// </summary>
    Json = 245,
        
    /// <summary>
    /// NEWDECIMAL
    /// </summary>
    NewDecimal = 246,
        
    /// <summary>
    /// ENUM
    /// </summary>
    Enum = 247,
        
    /// <summary>
    /// SET
    /// </summary>
    Set = 248,
        
    /// <summary>
    /// TINY_BLOB
    /// </summary>
    TinyBlob = 249,
        
    /// <summary>
    /// MEDIUM_BLOB
    /// </summary>
    MediumBlob = 250,
        
    /// <summary>
    /// LONG_BLOB
    /// </summary>
    LongBlob = 251,
        
    /// <summary>
    /// BLOB
    /// </summary>
    Blob = 252,
        
    /// <summary>
    /// VAR_STRING
    /// </summary>
    VarString = 253,
        
    /// <summary>
    /// STRING
    /// </summary>
    String = 254,
        
    /// <summary>
    /// GEOMETRY
    /// </summary>
    Geometry = 255
}