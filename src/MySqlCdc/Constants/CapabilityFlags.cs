namespace MySqlCdc.Constants;

/// <summary>
/// Server and client capability flags
/// <a href="https://mariadb.com/kb/en/library/connection/#capabilities">See more</a>
/// </summary>
[Flags]
internal enum CapabilityFlags
{
    LongPassword = 1 << 0,
    FoundRows = 1 << 1,
    LongFlag = 1 << 2,
    ConnectWithDb = 1 << 3,
    NoSchema = 1 << 4,
    Compress = 1 << 5,
    Odbc = 1 << 6,
    LocalFiles = 1 << 7,
    IgnoreSpace = 1 << 8,
    Protocol41 = 1 << 9,
    Interactive = 1 << 10,
    Ssl = 1 << 11,
    IgnoreSigpipe = 1 << 12,
    Transactions = 1 << 13,
    Reserved = 1 << 14,
    SecureConnection = 1 << 15,
    MultiStatements = 1 << 16,
    MultiResults = 1 << 17,
    PsMultiResults = 1 << 18,
    PluginAuth = 1 << 19
}