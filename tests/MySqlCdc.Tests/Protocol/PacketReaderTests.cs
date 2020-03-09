using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol
{
    public class PacketReaderTests
    {
        private static byte[] NumericPayload = new byte[]
        {
            250,  110,   10, 150,
             23,    0,   13, 255,
              3,   50,   80, 130,
            220,   75,  250,  78
        };

        [Fact]
        public void Test_ReadByte_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(NumericPayload[0], reader.ReadByte());
            Assert.Equal(1, reader.Consumed);

            Assert.Equal(NumericPayload[1], reader.ReadByte());
            Assert.Equal(2, reader.Consumed);
        }

        [Fact]
        public void Test_ReadUInt16LittleEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0x6EFA, reader.ReadUInt16LittleEndian());
            Assert.Equal(2, reader.Consumed);

            Assert.Equal(0x960A, reader.ReadUInt16LittleEndian());
            Assert.Equal(4, reader.Consumed);
        }

        [Fact]
        public void Test_ReadUInt16BigEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0xFA6E, reader.ReadUInt16BigEndian());
            Assert.Equal(2, reader.Consumed);

            Assert.Equal(0x0A96, reader.ReadUInt16BigEndian());
            Assert.Equal(4, reader.Consumed);
        }

        [Fact]
        public void Test_ReadUInt32LittleEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0x960A6EFA, reader.ReadUInt32LittleEndian());
            Assert.Equal(4, reader.Consumed);

            Assert.Equal(0xFF0D0017, reader.ReadUInt32LittleEndian());
            Assert.Equal(8, reader.Consumed);
        }

        [Fact]
        public void Test_ReadUInt32BigEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0xFA6E0A96, reader.ReadUInt32BigEndian());
            Assert.Equal(4, reader.Consumed);

            Assert.Equal((uint)0x17000DFF, reader.ReadUInt32BigEndian());
            Assert.Equal(8, reader.Consumed);
        }
    }
}
