using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Providers;

public class UserVarEventParserTests
{
    private static byte[] Payload = {
        0xc3, 0xe0, 0x1c, 0x5b, 0x0e, 0x01, 0x00, 0x00, 0x00, 0x2b, 0x00,
        0x00, 0x00, 0x2a, 0x02, 0x00, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x66, 0x6f, 0x6f, 0x00,
        0x00, 0x21, 0x00, 0x00, 0x00, 0x03, 0x00, 0x00, 0x00, 0x62, 0x61, 0x72, 0x6b, 0x3d, 0xd9, 0x7d
    };

    [Fact]
    public void Test_UserVarEvent_ReturnsEvent()
    {
        var reader = new PacketReader(Payload);
        var eventHeader = new EventHeader(ref reader);

        var parser = new UserVarEventParser();
        var @event = (UserVarEvent)parser.ParseEvent(eventHeader, ref reader);

        Assert.Equal("foo", @event.Name);
        Assert.NotNull(@event.Value);
        Assert.Equal(0x00, @event.Value.VariableType);
        Assert.Equal(33, @event.Value.CollationNumber);
        Assert.Equal("bar", @event.Value.Value);
    }
}