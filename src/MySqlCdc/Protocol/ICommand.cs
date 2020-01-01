namespace MySqlCdc.Protocol
{
    /// <summary>
    /// Represents MySql command that the client sends to the server.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Serializes client command to MySql packet.
        /// </summary>
        byte[] CreatePacket(byte sequenceNumber);
    }
}
