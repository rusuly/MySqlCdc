namespace MySqlCdc.Protocol;

/// <summary>
/// Represents MySql command that the client sends to the server.
/// </summary>
internal interface ICommand
{
    /// <summary>
    /// Serializes client command to MySql packet.
    /// </summary>
    byte[] Serialize();
}