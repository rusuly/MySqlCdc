using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
{
    /// <summary>
    /// Indicates a successful completion of a command.
    /// <see cref="https://mariadb.com/kb/en/library/ok_packet/"/>
    /// </summary>
    public class OkPacket : IPacket
    {
        public OkPacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);
        }
    }
}
