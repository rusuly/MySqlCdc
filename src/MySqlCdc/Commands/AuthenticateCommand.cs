using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using MySqlCdc.Constants;
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

    public AuthenticateCommand(ConnectionOptions options, int clientCollation, string scramble, string authPluginName, int maxPacketSize = 0)
    {
        ClientCollation = clientCollation;
        MaxPacketSize = maxPacketSize;
        Scramble = scramble;
        Username = options.Username;
        Password = options.Password;
        Database = options.Database;
        AuthPluginName = authPluginName;

        ClientCapabilities = (int)CapabilityFlags.LONG_FLAG
                             | (int)CapabilityFlags.PROTOCOL_41
                             | (int)CapabilityFlags.SECURE_CONNECTION
                             | (int)CapabilityFlags.PLUGIN_AUTH;

        if (Database != null)
            ClientCapabilities |= (int)CapabilityFlags.CONNECT_WITH_DB;
    }

    public byte[] CreatePacket(byte sequenceNumber)
    {
        var writer = new PacketWriter(sequenceNumber);
        writer.WriteIntLittleEndian(ClientCapabilities, 4);
        writer.WriteIntLittleEndian(MaxPacketSize, 4);
        writer.WriteIntLittleEndian(ClientCollation, 1);

        // Fill reserved bytes 
        for (int i = 0; i < 23; i++)
            writer.WriteByte(0);

        writer.WriteNullTerminatedString(Username);
        byte[] encryptedPassword = GetEncryptedPassword(Password, Scramble, AuthPluginName);
        writer.WriteByte((byte)encryptedPassword.Length);
        writer.WriteByteArray(encryptedPassword);

        if (Database != null)
            writer.WriteNullTerminatedString(Database);

        writer.WriteNullTerminatedString(AuthPluginName);
        return writer.CreatePacket();
    }

    public static byte[] GetEncryptedPassword(string password, string scramble, string authPluginName)
    {
        HashAlgorithm sha = authPluginName switch
        {
            AuthPluginNames.MySqlNativePassword => SHA1.Create(),
            AuthPluginNames.CachingSha2Password => SHA256.Create(),
            _ => throw new NotSupportedException()
        };

        var passwordHash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var concatHash = Encoding.UTF8.GetBytes(scramble).Concat(sha.ComputeHash(passwordHash)).ToArray();
        return Xor(passwordHash, sha.ComputeHash(concatHash));
    }

    public static byte[] Xor(byte[] array1, byte[] array2)
    {
        byte[] result = new byte[array1.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = (byte)(array1[i] ^ (array2[i % array2.Length]));
        return result;
    }
}