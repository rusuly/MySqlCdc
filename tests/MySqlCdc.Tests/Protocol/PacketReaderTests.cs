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
            220,   75,  250,  78,
             45,   99
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

        [Fact]
        public void Test_ReadInt64LittleEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0xFF0D0017960A6EFA, (ulong)reader.ReadInt64LittleEndian());
            Assert.Equal(8, reader.Consumed);

            Assert.Equal((ulong)0x4EFA4BDC82503203, (ulong)reader.ReadInt64LittleEndian());
            Assert.Equal(16, reader.Consumed);
        }

        [Fact]
        public void Test_ReadIntLittleEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0xFA, reader.ReadIntLittleEndian(1));
            Assert.Equal(1, reader.Consumed);

            Assert.Equal(0x0A6E, reader.ReadIntLittleEndian(2));
            Assert.Equal(3, reader.Consumed);

            Assert.Equal(0x001796, reader.ReadIntLittleEndian(3));
            Assert.Equal(6, reader.Consumed);
        }

        [Fact]
        public void Test_ReadLongLittleEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0x17960A6EFA, reader.ReadLongLittleEndian(5));
            Assert.Equal(5, reader.Consumed);

            Assert.Equal(0x503203FF0D00, reader.ReadLongLittleEndian(6));
            Assert.Equal(11, reader.Consumed);

            Assert.Equal(0x632D4EFA4BDC82, reader.ReadLongLittleEndian(7));
            Assert.Equal(18, reader.Consumed);
        }

        [Fact]
        public void Test_ReadIntBigEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0xFA, reader.ReadIntBigEndian(1));
            Assert.Equal(1, reader.Consumed);

            Assert.Equal(0x6E0A, reader.ReadIntBigEndian(2));
            Assert.Equal(3, reader.Consumed);

            Assert.Equal(0x961700, reader.ReadIntBigEndian(3));
            Assert.Equal(6, reader.Consumed);
        }

        [Fact]
        public void Test_ReadLongBigEndian_AdvancesOffset()
        {
            var reader = new PacketReader(NumericPayload);

            Assert.Equal(0xFA6E0A9617, reader.ReadLongBigEndian(5));
            Assert.Equal(5, reader.Consumed);

            Assert.Equal(0x000DFF033250, reader.ReadLongBigEndian(6));
            Assert.Equal(11, reader.Consumed);

            Assert.Equal(0x82DC4BFA4E2D63, reader.ReadLongBigEndian(7));
            Assert.Equal(18, reader.Consumed);
        }
    }
}
