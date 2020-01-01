using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
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
