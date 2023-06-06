using MySqlCdc.Constants;
using MySqlCdc.Network;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Network;

public class EventStreamChannelTests
{
    private const string SkipReason = "This test is CPU and Memory bound";

    [Fact(Skip = SkipReason)]
    public async Task Test_LargeSplitPacket_Combined()
    {
        using var stream = new MemoryStream();
        int lastPacketLength = 150;
        var packetBody = CreateLargePacket(stream, PacketConstants.MaxBodyLength * 4 + lastPacketLength);
        stream.Position = 0;

        var channel = new EventStreamChannel(new TestEventStreamReader(), stream);
        var packets = new List<IPacket>();

        await foreach (var packet in channel.ReadPacketAsync(TimeSpan.FromSeconds(30)))
        {
            packets.Add(packet);
        }

        Assert.Single(packets);
        Assert.IsType<TestPacket>(packets[0]);
        Assert.Equal(packetBody, ((TestPacket)packets[0]).Body);
    }

    [Fact(Skip = SkipReason)]
    public async Task Test_PacketExactly16MbWithEmptyPacket_Combined()
    {
        using var stream = new MemoryStream();
        var packetBody = CreateLargePacket(stream, PacketConstants.MaxBodyLength);
        WriteHeader(stream, 1, 0); // empty packet
        stream.Position = 0;

        var channel = new EventStreamChannel(new TestEventStreamReader(), stream);
        var packets = new List<IPacket>();

        await foreach (var packet in channel.ReadPacketAsync(TimeSpan.FromSeconds(30)))
        {
            packets.Add(packet);
        }

        Assert.Single(packets);
        Assert.IsType<TestPacket>(packets[0]);
        Assert.Equal(packetBody, ((TestPacket)packets[0]).Body);
    }

    private byte[] CreateLargePacket(MemoryStream stream, int bodySize)
    {
        byte sequence = 0;
        var packet = new byte[bodySize];
        for (int i = 0; i < packet.Length; i++)
        {
            packet[i] = (byte)(sequence + i % 256);
            if (i % PacketConstants.MaxBodyLength == 0)
            {
                sequence++;

                var packetLength = (packet.Length - i) > PacketConstants.MaxBodyLength
                    ? PacketConstants.MaxBodyLength
                    : packet.Length - i;

                WriteHeader(stream, sequence, packetLength);
            }
            stream.WriteByte(packet[i]);
        }
        return packet;
    }

    private void WriteHeader(MemoryStream stream, byte sequence, int packetLength)
    {
        for (int i = 0; i < 3; i++)
        {
            byte value = (byte)(0xFF & ((uint)packetLength >> (i << 3)));
            stream.WriteByte(value);
        }
        stream.WriteByte(sequence);
    }
}