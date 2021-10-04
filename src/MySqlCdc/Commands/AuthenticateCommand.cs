using MySqlCdc.Constants;
using MySqlCdc.Packets;
using MySqlCdc.Protocol;

namespace MySqlCdc.Commands;

/// <summary>
/// Client handshake response to the server initial handshake packet.
/// <a href="https://mariadb.com/kb/en/library/connection/#handshake-response-packet">See more</a>
/// </summary>
internal class AuthenticateCommand : ICommand
{
    public int ClientCapabilities { get; }
    public int ClientCollation { get; }
    public int MaxPacketSize { get; }
    public string Username { get; }
    public string Password { get; }
    public string Scramble { get; }
    public string? Database { get; }
    public string AuthPluginName { get; }

    public AuthenticateCommand(ConnectionOptions options, HandshakePacket handshake, int clientCollation)
    {
        ClientCollation = clientCollation;
        MaxPacketSize = 0;
        Scramble = handshake.Scramble;
        Username = options.Username;
        Password = options.Password;
        Database = options.Database;
        AuthPluginName = handshake.AuthPluginName;

        ClientCapabilities = (int) CapabilityFlags.LONG_FLAG
                             | (int) CapabilityFlags.PROTOCOL_41
                             | (int) CapabilityFlags.SECURE_CONNECTION
                             | (int) CapabilityFlags.PLUGIN_AUTH;

        if (Database != null)
            ClientCapabilities |= (int) CapabilityFlags.CONNECT_WITH_DB;
    }

    public byte[] Serialize()
    {
        var writer = new PacketWriter();
        writer.WriteIntLittleEndian(ClientCapabilities, 4);
        writer.WriteIntLittleEndian(MaxPacketSize, 4);
        writer.WriteIntLittleEndian(ClientCollation, 1);

        // Fill reserved bytes 
        for (int i = 0; i < 23; i++)
            writer.WriteByte(0);

        writer.WriteNullTerminatedString(Username);
        byte[] encryptedPassword = Extensions.GetEncryptedPassword(Password, Scramble, AuthPluginName);
        writer.WriteByte((byte) encryptedPassword.Length);
        writer.WriteByteArray(encryptedPassword);

        if (Database != null)
            writer.WriteNullTerminatedString(Database);

        writer.WriteNullTerminatedString(AuthPluginName);
        return writer.CreatePacket();
    }
}