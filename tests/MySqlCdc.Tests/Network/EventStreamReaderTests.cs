using System.Buffers;
using MySqlCdc.Checksum;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Packets;
using MySqlCdc.Providers.MySql;
using Xunit;

namespace MySqlCdc.Tests.Network;

public class EventStreamReaderTests
{
    [Fact]
    public void Test_EOFStatus_ReturnsEndOfFilePacket()
    {
        var reader = new EventStreamReader(new MySqlEventDeserializer());

        var payload = new byte[]
        {
            0xFE,
            3,0,2,8
        };
        var packet = reader.ReadPacket(new ReadOnlySequence<byte>(payload));

        Assert.IsType<EndOfFilePacket>(packet);

        var eofPacket = packet as EndOfFilePacket;
        Assert.Equal(3, eofPacket!.WarningCount);
        Assert.Equal(2 | (8 << 8), eofPacket.ServerStatus);
    }

    [Fact]
    public void Test_ErrorStatus_ReturnsErrorPacket()
    {
        var reader = new EventStreamReader(new MySqlEventDeserializer());

        var payload = new byte[]
        {
            0xFF,
            212,4,35,72,89,48,48,48,67,111,117,108,100,32,110,111,116,32,102,105,110,100,32,102,105,114,115,116, 32,108,111, 103,32,102,
            105,108,101,32,110,97,109,101,32,105,110,32,98,105,110,97,114,121,32,108,111,103,32,105,110,100,101,120,32,102,105,108,101
        };
        var packet = reader.ReadPacket(new ReadOnlySequence<byte>(payload));

        Assert.IsType<ErrorPacket>(packet);

        var errorPacket = packet as ErrorPacket;
        Assert.Equal("HY000", errorPacket!.SqlState);
        Assert.Equal(1236, errorPacket.ErrorCode);
        Assert.Equal("Could not find first log file name in binary log index file", errorPacket.ErrorMessage);
    }

    [Fact]
    public void Test_EventStatus_ReturnsEventPacket()
    {
        var reader = new EventStreamReader(new MySqlEventDeserializer()
        {
            ChecksumStrategy = new Crc32Checksum()
        });

        var payload = new byte[]
        {
            0x00,
            0,0,0,0,4,1,0,0,0,44,0,0,0,0,0,0,0,32,0,4,0,0,0,0,0,0,0,98,105,110,108,111,103,46,48,48,48,48,48,49,233,210,202,110
        };
        var packet = reader.ReadPacket(new ReadOnlySequence<byte>(payload));

        Assert.IsType<RotateEvent>(packet);

        var rotateEvent = packet as RotateEvent;
        Assert.Equal("binlog.000001", rotateEvent!.BinlogFilename);
        Assert.Equal(4, rotateEvent.BinlogPosition);
    }

    [Fact]
    public void Test_UnknownStatus_ReturnsExceptionPacket()
    {
        var reader = new EventStreamReader(new MySqlEventDeserializer());

        var payload = new byte[]
        {
            0xFD,
            3,0,2,8
        };

        var exception = Assert.Throws<Exception>(() => reader.ReadPacket(new ReadOnlySequence<byte>(payload)));
        Assert.Equal("Unknown network stream status", exception.Message);
    }


    [Fact]
    public void Test_DeserializationException_ReturnsExceptionPacket()
    {
        var reader = new EventStreamReader(new MySqlEventDeserializer()
        {
            ChecksumStrategy = new Crc32Checksum()
        });

        var payload = new byte[]
        {
            0,
            192,218,96,94,15,1,0,0,0,120,0,0,0,124,0,0,0,0,0,4,0,56,46,48,46,49,57,0,0,0,0,0,0,0,0,0,0,0,
            0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,192,218,96,94,19,0,13,0,8,0,
            0,0,0,4,0,4,0,0,0,96,0,4,26,8,0,0,0,8,8,8,2,0,0,0,10,10,10,42,42,0,18,52,0,10,
            5, // We changed checksum type
            225,100,86,201
        };

        var exception = Assert.Throws<InvalidOperationException>(() => reader.ReadPacket(new ReadOnlySequence<byte>(payload)));
        Assert.Equal("The master checksum type is not supported.", exception.Message);
    }
}