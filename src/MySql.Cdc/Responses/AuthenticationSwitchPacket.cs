using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
{
    /// <summary>
    /// Authentication Switch Request.
    /// <see cref="https://mariadb.com/kb/en/library/connection/#authentication-switch-request"/>
    /// </summary>
    public class AuthenticationSwitchPacket
    {
        public string AuthPluginName { get; private set; }
        public string AuthPluginData { get; private set; }

        public AuthenticationSwitchPacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);
            var header = reader.ReadInt(1);

            AuthPluginName = reader.ReadNullTerminatedString();
            AuthPluginData = reader.ReadNullTerminatedString();
        }
    }
}
