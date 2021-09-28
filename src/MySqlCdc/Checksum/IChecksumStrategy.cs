using System.Buffers;

namespace MySqlCdc.Checksum;

internal interface IChecksumStrategy
{
    /// <summary>
    /// Indicates the length of checksum appended to each event.
    /// </summary>
    int Length { get; }
        
    /// <summary>
    /// Verifies checksum of an event.
    /// </summary>
    bool Verify(ReadOnlySequence<byte> eventBuffer, ReadOnlySequence<byte> checksumBuffer);
}