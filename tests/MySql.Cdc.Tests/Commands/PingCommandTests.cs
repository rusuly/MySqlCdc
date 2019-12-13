using MySql.Cdc.Commands;
using Xunit;

namespace MySql.Cdc.Tests.Commands
{
    public class PingCommandTests
    {
        [Fact]
        public void Test_PingCommand_CreatesValidPacket()
        {
            var command = new PingCommand();
            var packet = command.CreatePacket(3);
            Assert.Equal(new byte[] { 0x01, 0x00, 0x00, 0x03, 0x0E }, packet);
        }
    }
}
