using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
{
    /// <summary>
    /// EOF packet marks the end of a resultset and returns status and warnings.
    /// <see cref="https://mariadb.com/kb/en/library/eof_packet/"/>
    /// </summary>
    public class EndOfFilePacket : IPacket
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
