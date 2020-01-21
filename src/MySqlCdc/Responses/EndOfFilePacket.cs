using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
{
    /// <summary>
    /// EOF packet marks the end of a resultset and returns status and warnings.
    /// <a href="https://mariadb.com/kb/en/library/eof_packet/">See more</a>
    /// </summary>
    internal class EndOfFilePacket : IPacket
    {
        public int WarningCount { get; private set; }
        public int ServerStatus { get; private set; }

        public EndOfFilePacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);

            WarningCount = reader.ReadInt(2);
            ServerStatus = reader.ReadInt(2);
        }
    }
}
