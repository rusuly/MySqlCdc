using System.Buffers;
using System.Linq;
using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
{
    /// <summary>
    /// Initial handshake packet sent by the server.
    /// <a href="https://mariadb.com/kb/en/library/connection/#initial-handshake-packet">See more</a>
    /// </summary>
    internal class HandshakePacket : IPacket
    {
        public int ProtocolVersion { get; }
        public string ServerVersion { get; }
        public int ConnectionId { get; }
        public string Scramble { get; }
        public long ServerCapabilities { get; }
        public int ServerCollation { get; }
        public int StatusFlags { get; }
        public string Filler { get; }
        public int AuthPluginLength { get; }
        public string AuthPluginName { get; }

        public HandshakePacket(ReadOnlySequence<byte> buffer)
        {
            using var memoryOwner = new MemoryOwner(buffer);
            var reader = new PacketReader(memoryOwner.Memory);

            ProtocolVersion = reader.ReadInt(1);
            ServerVersion = reader.ReadNullTerminatedString();
            ConnectionId = reader.ReadInt(4);
            Scramble = reader.ReadNullTerminatedString();
            byte[] capabilityFlags1 = reader.ReadByteArraySlow(2);
            ServerCollation = reader.ReadInt(1);
            StatusFlags = reader.ReadInt(2);
            byte[] capabilityFlags2 = reader.ReadByteArraySlow(2);
            AuthPluginLength = reader.ReadInt(1);
            Filler = reader.ReadString(6);
            byte[] capabilityFlags3 = reader.ReadByteArraySlow(4);

            // Join lower and upper capability flags to a number
            var capabilityFlags = capabilityFlags1.Concat(capabilityFlags2).Concat(capabilityFlags3).ToArray();
            for (int i = 0; i < capabilityFlags.Length; i++)
            {
                ServerCapabilities |= (long)capabilityFlags[i] << (i << 3);
            }

            // Handle specific conditions
            if ((ServerCapabilities & (int)CapabilityFlags.SECURE_CONNECTION) > 0)
            {
                Scramble += reader.ReadNullTerminatedString();
            }
            if ((ServerCapabilities & (int)CapabilityFlags.PLUGIN_AUTH) > 0)
            {
                AuthPluginName = reader.ReadNullTerminatedString();
            }
        }
    }
}
