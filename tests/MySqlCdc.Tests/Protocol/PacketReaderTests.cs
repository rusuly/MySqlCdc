using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol
{
    public class PacketReaderTests
    {
        [Fact]
        public void ReadByte_FromSpan_ReturnsByte()
        {
            var array = new byte[] { 250, 110 };
            var reader = new PacketReader(array);

            Assert.Equal(array[0], reader.ReadByte());
            Assert.Equal(1, reader.Consumed);

            Assert.Equal(array[1], reader.ReadByte());
            Assert.Equal(2, reader.Consumed);
        }
    }
}
