using System.Buffers;

namespace MySqlCdc.Checksum
{
    internal class NoneChecksum : IChecksumStrategy
    {
        public int Length => 0;

        public bool Verify(ReadOnlySequence<byte> eventBuffer, ReadOnlySequence<byte> checksumBuffer)
        {
            return true;
        }
    }
}
