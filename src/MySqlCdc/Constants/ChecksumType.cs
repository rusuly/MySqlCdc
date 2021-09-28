namespace MySqlCdc.Constants;

/// <summary>
/// Checksum type used in a binlog file.
/// </summary>
public enum ChecksumType
{
    /// <summary>
    /// Checksum is disabled.
    /// </summary>
    NONE = 0,

    /// <summary>
    /// CRC32 checksum.
    /// </summary>
    CRC32 = 1
}