using MySqlCdc.Events;
using MySqlCdc.Protocol;
using MySqlCdc.Providers.MySql;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class XaPrepareEventParserTests
    {
        private static byte[] Payload = { 0, 123, 0, 0, 0, 5, 0, 0, 0, 5, 0, 0, 0, 103, 116, 114, 105, 100, 98, 113, 117, 97, 108 };

        [Fact]
        public void Test_ParseXaPrepareEvent_ReturnsEvent()
        {
            var eventHeader = CreateEventHeader();
            
            var reader = new PacketReader(Payload);
            var parser = new XaPrepareEventParser();
            var @event = (XaPrepareEvent)parser.ParseEvent(eventHeader, ref reader);

            Assert.False(@event.OnePhase);
            Assert.Equal(123, @event.FormatId);
            Assert.Equal("gtrid", @event.Gtrid);
            Assert.Equal("bqual", @event.Bqual);
        }
        
        private EventHeader CreateEventHeader()
        {
            var reader = new PacketReader(Payload);
            return new EventHeader(ref reader);
        }
    }
}
