using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
{
    /// <summary>
    /// Authentication Switch Request.
    /// <a href="https://mariadb.com/kb/en/library/connection/#authentication-switch-request">See more</a>
    /// </summary>
    internal class AuthPluginSwitchPacket : IPacket
    {
        public string AuthPluginName { get; }
        public string AuthPluginData { get; }

        public AuthPluginSwitchPacket(ReadOnlySequence<byte> buffer)
        {
            using var memoryOwner = new MemoryOwner(buffer);
            var reader = new PacketReader(memoryOwner.Memory);

            AuthPluginName = reader.ReadNullTerminatedString();
            AuthPluginData = reader.ReadNullTerminatedString();
        }
    }
}
