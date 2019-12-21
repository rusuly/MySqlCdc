using System.Buffers;

namespace MySql.Cdc.Checksum
{
    public interface IChecksumStrategy
    {
        /// <summary>
        /// Indicates the length of checksum appended to each event.
        /// </summary>
        int Length { get; }
        
        bool Verify(ReadOnlySequence<byte> eventBuffer, ReadOnlySequence<byte> checksumBuffer);
    }
}
