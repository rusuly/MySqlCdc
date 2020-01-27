using System.Buffers;

namespace MySqlCdc.Checksum
{
    internal class Crc32Checksum : IChecksumStrategy
    {
        public int Length => 4;

        public bool Verify(ReadOnlySequence<byte> eventBuffer, ReadOnlySequence<byte> checksumBuffer)
        {
            throw new System.NotImplementedException();
        }
    }
}
