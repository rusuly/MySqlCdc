namespace MySqlCdc.Constants;

/// <summary>
/// Represents the SSL strategy used to connect to the server. 
/// </summary>
public enum SslMode
{
    /// <summary>
    /// Establishes an unencrypted connection. 
    /// </summary>
    DISABLED,

    /// <summary>
    /// Tries to establish an encrypted connection without verifying CA/Host. Falls back to an unencrypted connection.
    /// </summary>
    IF_AVAILABLE,

    /// <summary>
    /// Require an encrypted connection without verifying CA/Host.
    /// </summary>
    REQUIRE,

    /// <summary>
    /// Verify that the certificate belongs to the Certificate Authority.
    /// </summary>
    REQUIRE_VERIFY_CA,

    /// <summary>
    /// Verify that the certificate belongs to the Certificate Authority and matches Host.
    /// </summary>
    REQUIRE_VERIFY_FULL
}