using MySqlCdc.Columns;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class ColumnParserTests
    {
        private ColumnParser _columnParser = new ColumnParser();

        [Fact]
        public void Test_TinyInt_Positive()
        {
            byte[] payload = new byte[] { 127 };
            var reader = new PacketReader(payload);
            Assert.Equal(127, (sbyte)_columnParser.ParseTinyInt(ref reader, 0));
        }

        [Fact]
        public void Test_TinyInt_Negative()
        {
            byte[] payload = new byte[] { 128 };
            var reader = new PacketReader(payload);
            Assert.Equal(-128, (sbyte)_columnParser.ParseTinyInt(ref reader, 0));
        }

        [Fact]
        public void Test_SmallInt_Positive()
        {
            byte[] payload = new byte[] { 255, 127 };
            var reader = new PacketReader(payload);
            Assert.Equal(32767, _columnParser.ParseSmallInt(ref reader, 0));
        }

        [Fact]
        public void Test_SmallInt_Negative()
        {
            byte[] payload = new byte[] { 0, 128 };
            var reader = new PacketReader(payload);
            Assert.Equal(-32768, _columnParser.ParseSmallInt(ref reader, 0));
        }

        [Fact]
        public void Test_MediumInt_Positive()
        {
            byte[] payload = new byte[] { 255, 255, 127 };
            var reader = new PacketReader(payload);
            Assert.Equal(8388607, _columnParser.ParseMediumInt(ref reader, 0));
        }

        [Fact]
        public void Test_MediumInt_Negative()
        {
            byte[] payload = new byte[] { 0, 0, 128 };
            var reader = new PacketReader(payload);
            Assert.Equal(-8388608, _columnParser.ParseMediumInt(ref reader, 0));
        }

        [Fact]
        public void Test_Int_Positive()
        {
            byte[] payload = new byte[] { 255, 255, 255, 127 };
            var reader = new PacketReader(payload);
            Assert.Equal(2147483647, _columnParser.ParseInt(ref reader, 0));
        }

        [Fact]
        public void Test_Int_Negative()
        {
            byte[] payload = new byte[] { 0, 0, 0, 128 };
            var reader = new PacketReader(payload);
            Assert.Equal(-2147483648, _columnParser.ParseInt(ref reader, 0));
        }

        [Fact]
        public void Test_BigInt_Positive()
        {
            byte[] payload = new byte[] { 255, 255, 255, 255, 255, 255, 255, 127 };
            var reader = new PacketReader(payload);
            Assert.Equal(9223372036854775807, _columnParser.ParseBigInt(ref reader, 0));
        }

        [Fact]
        public void Test_BigInt_Negative()
        {
            byte[] payload = new byte[] { 0, 0, 0, 0, 0, 0, 0, 128 };
            var reader = new PacketReader(payload);
            Assert.Equal(-9223372036854775808, _columnParser.ParseBigInt(ref reader, 0));
        }

        [Fact]
        public void Test_Float_Positive()
        {
            byte[] payload = new byte[] { 121, 233, 246, 66 };
            var reader = new PacketReader(payload);
            Assert.Equal(123.456, _columnParser.ParseFloat(ref reader, 0));
        }

        [Fact]
        public void Test_Float_Negative()
        {
            byte[] payload = new byte[] { 121, 233, 246, 194 };
            var reader = new PacketReader(payload);
            Assert.Equal(-123.456, _columnParser.ParseFloat(ref reader, 0));
        }

        [Fact]
        public void Test_Double_Positive()
        {
            byte[] payload = new byte[] { 196, 34, 101, 84, 52, 111, 157, 65 };
            var reader = new PacketReader(payload);
            Assert.Equal(123456789.09876543, _columnParser.ParseDouble(ref reader, 0));
        }

        [Fact]
        public void Test_Double_Negative()
        {
            byte[] payload = new byte[] { 196, 34, 101, 84, 52, 111, 157, 193 };
            var reader = new PacketReader(payload);
            Assert.Equal(-123456789.09876543, _columnParser.ParseDouble(ref reader, 0));
        }
    }
}
