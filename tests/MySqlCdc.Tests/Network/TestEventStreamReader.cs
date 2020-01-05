using System.Buffers;
using System.Threading.Tasks;
using MySqlCdc.Network;
using MySqlCdc.Protocol;

namespace MySqlCdc.Tests.Network
{
    public class TestEventStreamReader : IEventStreamReader
    {
        public async Task<IPacket> ReadPacketAsync(ReadOnlySequence<byte> buffer)
        {
            return new TestPacket(buffer);
        }
    }
}
