using System.Buffers;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol
{
    public class ReaderWriterTests : IPacket
    {
        [Fact]
        public void Test_WrittenTypes_ReadCorrectly()
        {
            const int int8 = 123;
            const int int16 = 12345;
            const int int24 = 1234567;
            const int int32 = 2123456789;

            var writer = new PacketWriter(0);

            writer.WriteInt(int8, 1);
            writer.WriteInt(int16, 2);
            writer.WriteInt(int24, 3);
            writer.WriteInt(int32, 4);

            var reader = new PacketReader(new ReadOnlySequence<byte>(writer.CreatePacket()));

            //Read packet size and sequence number
            reader.ReadInt(3);
            reader.ReadInt(1);

            Assert.Equal(int8, reader.ReadInt(1));
            Assert.Equal(int16, reader.ReadInt(2));
            Assert.Equal(int24, reader.ReadInt(3));
            Assert.Equal(int32, reader.ReadInt(4));
        }
    }
}
