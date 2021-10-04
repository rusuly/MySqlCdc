using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Commands;

/// <summary>
/// SSLRequest packet used in SSL/TLS connection.
/// <a href="https://mariadb.com/kb/en/library/connection/#sslrequest-packet">See more</a>
/// </summary>
internal class SslRequestCommand : ICommand
{
    public int ClientCapabilities { get; }
    public int ClientCollation { get; }
    public int MaxPacketSize { get; }

    public SslRequestCommand(int clientCollation)
    {
        ClientCollation = clientCollation;
        MaxPacketSize = 0;

        ClientCapabilities = (int)CapabilityFlags.LONG_FLAG
                             | (int)CapabilityFlags.PROTOCOL_41
                             | (int)CapabilityFlags.SECURE_CONNECTION
                             | (int)CapabilityFlags.SSL
                             | (int)CapabilityFlags.PLUGIN_AUTH;
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

        return writer.CreatePacket();
    }
}