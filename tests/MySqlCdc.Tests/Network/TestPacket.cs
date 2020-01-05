using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Tests.Network
{
    public class TestPacket : IPacket
    {
        public byte[] Body { get; }

        public TestPacket(ReadOnlySequence<byte> sequence)
        {
            Body = sequence.ToArray();
        }
    }
}
