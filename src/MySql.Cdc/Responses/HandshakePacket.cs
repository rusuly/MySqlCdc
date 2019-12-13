using System.Buffers;
using System.Linq;
using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
{
    /// <summary>
    /// Initial handshake packet sent by the server.
    /// <see cref="https://mariadb.com/kb/en/library/connection/#initial-handshake-packet"/>
    /// </summary>
    public class HandshakePacket
    {
        public int ProtocolVersion { get; private set; }
        public string ServerVersion { get; private set; }
        public int ConnectionId { get; private set; }
        public string Scramble { get; private set; }
        public long ServerCapabilities { get; private set; }
        public int ServerCollation { get; private set; }
        public int StatusFlags { get; private set; }
        public string Filler { get; private set; }
        public int AuthPluginLength { get; private set; }
        public string AuthPluginName { get; private set; }

        public HandshakePacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);

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
