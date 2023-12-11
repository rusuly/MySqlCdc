
using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Parsers;

public class RandEventParserTests
{
    private static byte[] Payload = {
        0x5f, 0x56, 0x75, 0x65, 0x0d, 0x01, 0x00, 0x00, 0x00, 0x27, 0x00,
        0x00, 0x00, 0x02, 0x07, 0x00, 0x00, 0x00, 0x00, 0x0c, 0xdb, 0x85, 0x2d, 0x00, 0x00,
        0x00, 0x00, 0x07, 0x97, 0xbc, 0x04, 0x00, 0x00, 0x00, 0x00, 0xa4, 0x6e, 0x46, 0xdd
    };

    [Fact]
    public void Test_RandEvent_ReturnsEvent()
    {
        var reader = new PacketReader(Payload);
        var eventHeader = EventHeader.Read(ref reader);

        var parser = new RandEventParser();
        var @event = (RandEvent)parser.ParseEvent(eventHeader, ref reader);
        ;
        Assert.Equal(763747084UL, @event.Seed1);
        Assert.Equal(79468295UL, @event.Seed2);
    }
}