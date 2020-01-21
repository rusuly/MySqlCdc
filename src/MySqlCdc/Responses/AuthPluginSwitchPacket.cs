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
        public string AuthPluginName { get; private set; }
        public string AuthPluginData { get; private set; }

        public AuthPluginSwitchPacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);

            AuthPluginName = reader.ReadNullTerminatedString();
            AuthPluginData = reader.ReadNullTerminatedString();
        }
    }
}
