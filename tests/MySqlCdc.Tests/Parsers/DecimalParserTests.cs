using MySqlCdc.Columns;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class DecimalParserTests
    {
        private ColumnParser _columnParser = new ColumnParser();

        [Fact]
        public void Test_ParseNewDecimal()
        {
            // decimal(65,10), column = '1234567890112233445566778899001112223334445556667778889.9900011112'
            byte[] payload = new byte[]
            {
                65, 10,
                129, 13, 251, 56, 210, 6, 176, 139, 229, 33, 200, 92, 19, 0, 16, 248, 159, 19, 239, 59, 244, 39, 205, 127, 73, 59, 2, 55, 215, 2
            };
            var reader = new PacketReader(payload);
            int metadata = reader.ReadIntLittleEndian(2);

            var expected = "1234567890112233445566778899001112223334445556667778889.9900011112";
            Assert.Equal(expected, _columnParser.ParseNewDecimal(ref reader, metadata));
        }

        [Fact]
        public void Test_ParseNewDecimal_Negative()
        {
            // decimal(65,10), column = '-1234567890112233445566778899001112223334445556667778889.9900011112'
            byte[] payload = new byte[]
            {
                65, 10,
                126, 242, 4, 199, 45, 249, 79, 116, 26, 222, 55, 163, 236, 255, 239, 7, 96, 236, 16, 196, 11, 216, 50, 128, 182, 196, 253, 200, 40, 253
            };
            var reader = new PacketReader(payload);
            int metadata = reader.ReadIntLittleEndian(2);

            var expected = "-1234567890112233445566778899001112223334445556667778889.9900011112";
            Assert.Equal(expected, _columnParser.ParseNewDecimal(ref reader, metadata));
        }

        [Fact]
        public void Test_ParseNewDecimal_StartingZerosIgnored()
        {
            // decimal(65,10), column = '7778889.9900011112'
            byte[] payload = new byte[]
            {
                65, 10,
                128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 118, 178, 73, 59, 2, 55, 215, 2
            };
            var reader = new PacketReader(payload);
            int metadata = reader.ReadIntLittleEndian(2);

            var expected = "7778889.9900011112";
            Assert.Equal(expected, _columnParser.ParseNewDecimal(ref reader, metadata));
        }

        [Fact]
        public void Test_ParseNewDecimal_HasIntegralZero()
        {
            // decimal(65,10), column = '.9900011112'
            byte[] payload = new byte[]
            {
                65, 10,
                128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 59, 2, 55, 215, 2
            };
            var reader = new PacketReader(payload);
            int metadata = reader.ReadIntLittleEndian(2);

            var expected = "0.9900011112";
            Assert.Equal(expected, _columnParser.ParseNewDecimal(ref reader, metadata));
        }

        [Fact]
        public void Test_CompressedFractional_StartingZerosPreserved()
        {
            // In this test first two zeros are preserved->[uncompr][comp]
            // decimal(60,15), column = '34445556667778889.123456789006700'
            byte[] payload = new byte[]
            {
                60, 15,
                128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 13, 152, 244, 39, 205, 127, 73, 7, 91, 205, 21, 0, 26, 44
            };
            var reader = new PacketReader(payload);
            int metadata = reader.ReadIntLittleEndian(2);

            var expected = "34445556667778889.123456789006700";
            Assert.Equal(expected, _columnParser.ParseNewDecimal(ref reader, metadata));
        }

        [Fact]
        public void Test_ParseNewDecimal_Integer()
        {
            // decimal(60,0), column = '34445556667778889'
            byte[] payload = new byte[]
            {
                60, 0,
                128, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 13, 152, 244, 39, 205, 127, 73
            };
            var reader = new PacketReader(payload);
            int metadata = reader.ReadIntLittleEndian(2);

            var expected = "34445556667778889";
            Assert.Equal(expected, _columnParser.ParseNewDecimal(ref reader, metadata));
        }
    }
}
