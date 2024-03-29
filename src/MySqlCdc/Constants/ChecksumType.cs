namespace MySqlCdc.Constants;

/// <summary>
/// Checksum type used in a binlog file.
/// </summary>
public enum ChecksumType
{
    /// <summary>
    /// Checksum is disabled.
    /// </summary>
    None = 0,

    /// <summary>
    /// CRC32 checksum.
    /// </summary>
    Crc32 = 1
}