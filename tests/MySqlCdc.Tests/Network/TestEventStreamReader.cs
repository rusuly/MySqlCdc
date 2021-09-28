using System.Buffers;
using MySqlCdc.Network;
using MySqlCdc.Protocol;

namespace MySqlCdc.Tests.Network;

public class TestEventStreamReader : IEventStreamReader
{
    public IPacket ReadPacket(ReadOnlySequence<byte> buffer)
    {
        return new TestPacket(buffer);
    }
}