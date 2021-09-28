using System;
using System.Buffers;

namespace MySqlCdc.Network;

internal class PacketSegment : ReadOnlySequenceSegment<byte>
{
    public PacketSegment(ReadOnlyMemory<byte> memory)
    {
        Memory = memory;
    }

    public PacketSegment Add(ReadOnlyMemory<byte> memory)
    {
        var segment = new PacketSegment(memory)
        {
            RunningIndex = this.RunningIndex + this.Memory.Length
        };
        Next = segment;
        return segment;
    }
}