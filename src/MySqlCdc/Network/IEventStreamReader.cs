using System.Buffers;
using System.Threading.Tasks;
using MySqlCdc.Protocol;

namespace MySqlCdc.Network
{
    public interface IEventStreamReader
    {
        Task<IPacket> ReadPacketAsync(ReadOnlySequence<byte> buffer);
    }
}
