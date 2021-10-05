namespace MySqlCdc.Constants;

/// <summary>
/// Represents the SSL strategy used to connect to the server. 
/// </summary>
public enum SslMode
{
    /// <summary>
    /// Establishes an unencrypted connection. 
    /// </summary>
    Disabled = 1,

    /// <summary>
    /// Tries to establish an encrypted connection without verifying CA/Host. Falls back to an unencrypted connection.
    /// </summary>
    IfAvailable = 2,

    /// <summary>
    /// Require an encrypted connection without verifying CA/Host.
    /// </summary>
    Require = 3,

    /// <summary>
    /// Verify that the certificate belongs to the Certificate Authority.
    /// </summary>
    RequireVerifyCa = 4,

    /// <summary>
    /// Verify that the certificate belongs to the Certificate Authority and matches Host.
    /// </summary>
    RequireVerifyFull = 5
}