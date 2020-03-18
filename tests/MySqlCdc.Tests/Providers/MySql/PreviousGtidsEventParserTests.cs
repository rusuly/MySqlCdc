using MySqlCdc.Events;
using MySqlCdc.Protocol;
using MySqlCdc.Providers.MySql;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class PreviousGtidsEventParserTests
    {
        private static byte[] Payload = new byte[]
        {
            2, 0, 0, 0, 0, 0, 0, 0, 181, 205, 22, 36, 95, 48, 17, 228, 180, 233, 16, 81, 114, 27,
            210, 65, 1, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 241, 15, 108, 0, 0, 0, 0, 0,
            187, 66, 29, 38, 95, 48, 17, 228, 180, 233, 216, 157, 103, 43, 46, 248, 1,
            0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 209, 97, 119, 0, 0, 0, 0, 0
        };
        private const string GtidSet = "b5cd1624-5f30-11e4-b4e9-1051721bd241:1-7081968,bb421d26-5f30-11e4-b4e9-d89d672b2ef8:1-7823824";

        [Fact]
        public void Test_ParsePreviousGtidsEvent_ReturnsGtidSet()
        {
            var reader = new PacketReader(Payload);
            var parser = new PreviousGtidsEventParser();
            var @event = (PreviousGtidsEvent)parser.ParseEvent(null, ref reader);

            Assert.Equal(GtidSet, @event.GtidSet.ToString());
        }
    }
}
