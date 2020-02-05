using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
{
    /// <summary>
    /// Indicates a successful completion of a command.
    /// <a href="https://mariadb.com/kb/en/library/ok_packet/">See more</a>
    /// </summary>
    internal class OkPacket : IPacket
    {
        public OkPacket(ReadOnlySequence<byte> buffer)
        {
        }
    }
}
