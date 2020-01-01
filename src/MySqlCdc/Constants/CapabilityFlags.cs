using System;

namespace MySqlCdc.Constants
{
    /// <summary>
    /// Server and client capability flags
    /// <see cref="https://mariadb.com/kb/en/library/connection/#capabilities"/>
    /// </summary>
    [Flags]
    public enum CapabilityFlags
    {
        LONG_PASSWORD = 1 << 0,
        FOUND_ROWS = 1 << 1,
        LONG_FLAG = 1 << 2,
        CONNECT_WITH_DB = 1 << 3,
        NO_SCHEMA = 1 << 4,
        COMPRESS = 1 << 5,
        ODBC = 1 << 6,
        LOCAL_FILES = 1 << 7,
        IGNORE_SPACE = 1 << 8,
        PROTOCOL_41 = 1 << 9,
        INTERACTIVE = 1 << 10,
        SSL = 1 << 11,
        IGNORE_SIGPIPE = 1 << 12,
        TRANSACTIONS = 1 << 13,
        RESERVED = 1 << 14,
        SECURE_CONNECTION = 1 << 15,
        MULTI_STATEMENTS = 1 << 16,
        MULTI_RESULTS = 1 << 17,
        PS_MULTI_RESULTS = 1 << 18,
        PLUGIN_AUTH = 1 << 19
    }
}
