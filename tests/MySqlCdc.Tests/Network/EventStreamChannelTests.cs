using System.IO;
using System.Threading.Tasks;
using MySqlCdc.Constants;
using MySqlCdc.Network;
using Xunit;

namespace MySqlCdc.Tests.Network
{
    public class EventStreamChannelTests
    {
        [Fact]
        public async Task Test_SplitPacket_Combined()
        {
            using var stream = new MemoryStream();
            int lastPacketLength = 150;
            var packetBody = CreateLargePacket(stream, PacketConstants.MaxBodyLength * 4 + lastPacketLength);
            stream.Position = 0;

            var channel = new EventStreamChannel(new TestEventStreamReader(), stream);
            var packet = await channel.ReadPacketAsync();
            Assert.IsType<TestPacket>(packet);
            Assert.Equal(packetBody, ((TestPacket)packet).Body);
        }

        [Fact]
        public async Task Test_PacketExactly16MbWithEmptyPacket_Combined()
        {
            using var stream = new MemoryStream();
            var packetBody = CreateLargePacket(stream, PacketConstants.MaxBodyLength);
            WriteHeader(stream, 1, 0); // empty packet
            stream.Position = 0;

            var channel = new EventStreamChannel(new TestEventStreamReader(), stream);
            var packet = await channel.ReadPacketAsync();
            Assert.IsType<TestPacket>(packet);
            Assert.Equal(packetBody, ((TestPacket)packet).Body);
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
}
