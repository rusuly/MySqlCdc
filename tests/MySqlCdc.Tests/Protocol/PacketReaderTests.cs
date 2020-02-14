using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol
{
    public class PacketReaderTests
    {
        [Fact]
        public void ReadByte_FromSpan_ReturnsInt8()
        {
            var array = new byte[] { 250, 110, 54 };
            var reader = new PacketReader(array);

            Assert.Equal(250, reader.ReadByte());
            Assert.Equal(1, reader.Consumed);

            Assert.Equal(110, reader.ReadByte());
            Assert.Equal(2, reader.Consumed);

            Assert.Equal(54, reader.ReadByte());
            Assert.Equal(3, reader.Consumed);
        }
    }
}
