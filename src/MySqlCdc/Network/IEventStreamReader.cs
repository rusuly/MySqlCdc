using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Network
{
    public interface IEventStreamReader
    {
        IPacket ReadPacket(ReadOnlySequence<byte> buffer);
    }
}
