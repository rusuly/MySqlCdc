using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Network;

internal interface IEventStreamReader
{
    IPacket ReadPacket(ReadOnlySequence<byte> buffer);
}