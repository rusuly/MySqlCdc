using System;
using System.Buffers;

namespace MySqlCdc.Protocol
{
    internal class MemoryOwner : IDisposable
    {
        private readonly byte[] _lease;
        public ReadOnlyMemory<byte> Memory { get; }

        public MemoryOwner(ReadOnlySequence<byte> sequence)
        {
            _lease = ArrayPool<byte>.Shared.Rent((int)sequence.Length);
            sequence.CopyTo(_lease);
            Memory = new ReadOnlyMemory<byte>(_lease, 0, (int)sequence.Length);
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(_lease);
        }
    }
}
