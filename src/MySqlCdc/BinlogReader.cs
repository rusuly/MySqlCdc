using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.CompilerServices;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc;

/// <summary>
/// Reads binlog events from a stream.
/// </summary>
public class BinlogReader
{
    private static readonly byte[] MagicNumber = { 0xfe, 0x62, 0x69, 0x6e };
    private readonly EventDeserializer _eventDeserializer;
    private readonly PipeReader _pipeReader;

    /// <summary>
    /// Creates a new <see cref="BinlogReader"/>.
    /// </summary>
    /// <param name="eventDeserializer">EventDeserializer implementation for a specific provider</param>
    /// <param name="stream">Stream representing a binlog file</param>
    public BinlogReader(EventDeserializer eventDeserializer, Stream stream)
    {
        byte[] header = new byte[EventConstants.FirstEventPosition];
        stream.Read(header, 0, EventConstants.FirstEventPosition);

        if (!header.SequenceEqual(MagicNumber))
            throw new InvalidOperationException("Invalid binary log file header");

        _eventDeserializer = eventDeserializer;
        _pipeReader = PipeReader.Create(stream);
    }

    /// <summary>
    /// Reads an event from binlog stream.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Binlog event instance</returns>
    public async IAsyncEnumerable<(EventHeader, IBinlogEvent)> ReadEvents([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ReadResult result = await _pipeReader.ReadAsync(cancellationToken);
            ReadOnlySequence<byte> buffer = result.Buffer;

            while (!cancellationToken.IsCancellationRequested)
            {
                // We can't calculate packet size without the event header
                if (buffer.Length < EventConstants.HeaderSize)
                    break;

                // Make sure the event fits in the buffer
                var eventHeader = GetEventHeader(buffer);
                if (buffer.Length < eventHeader.EventLength)
                    break;

                // Process event and repeat in case there are more event in the buffer
                var packet = buffer.Slice(0, eventHeader.EventLength);
                var binlogEvent = Deserialize(packet);
                yield return (binlogEvent.Header, binlogEvent.Event);

                buffer = buffer.Slice(buffer.GetPosition(eventHeader.EventLength));
            }

            _pipeReader.AdvanceTo(buffer.Start, buffer.End);

            if (result.IsCompleted)
                break;
        }

        await _pipeReader.CompleteAsync();
    }

    private EventHeader GetEventHeader(ReadOnlySequence<byte> buffer)
    {
        using var memoryOwner = new MemoryOwner(buffer.Slice(0, EventConstants.HeaderSize));
        var reader = new PacketReader(memoryOwner.Memory.Span);
        return new EventHeader(ref reader);
    }

    private HeaderWithEvent Deserialize(ReadOnlySequence<byte> packet)
    {
        using var memoryOwner = new MemoryOwner(packet);
        var reader = new PacketReader(memoryOwner.Memory.Span);
        return _eventDeserializer.DeserializeEvent(ref reader);
    }
}