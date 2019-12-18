using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
{
    /// <summary>
    /// Authentication Switch Request.
    /// <see cref="https://mariadb.com/kb/en/library/connection/#authentication-switch-request"/>
    /// </summary>
    public class AuthPluginSwitchPacket : IPacket
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
