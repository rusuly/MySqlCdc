using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Packets;
using MySqlCdc.Protocol;

namespace MySqlCdc
{
    /// <summary>
    /// Reads binlog events from a stream.
    /// </summary>
    public class BinlogReader
    {
        private static byte[] MagicNumber = new byte[] { 0xfe, 0x62, 0x69, 0x6e };
        private readonly Channel<IPacket> _channel = Channel.CreateBounded<IPacket>(
            new BoundedChannelOptions(100)
            {
                SingleReader = true,
                SingleWriter = true
            });

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
            _ = Task.Run(async () => await ReceivePacketsAsync(_pipeReader));
        }

        private async Task ReceivePacketsAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (true)
                {
                    // We can't calculate packet size without the event header
                    if (buffer.Length < EventConstants.HeaderSize)
                        break;

                    // Make sure the event fits in the buffer
                    var eventHeader = new EventHeader(buffer.Slice(0, EventConstants.HeaderSize));
                    if (buffer.Length < eventHeader.EventLength)
                        break;

                    // Process event and repeat in case there are more event in the buffer
                    await OnReceiveEvent(buffer.Slice(0, eventHeader.EventLength));
                    buffer = buffer.Slice(buffer.GetPosition(eventHeader.EventLength));
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                    break;
            }

            await reader.CompleteAsync();
            _channel.Writer.Complete();
        }

        private async Task OnReceiveEvent(ReadOnlySequence<byte> buffer)
        {
            try
            {
                var @event = _eventDeserializer.DeserializeEvent(buffer);
                await _channel.Writer.WriteAsync(@event);
            }
            catch (Exception e)
            {
                // We stop replication if deserialize throws an exception 
                // Since a derived database may end up in an inconsistent state.
                await _channel.Writer.WriteAsync(new ExceptionPacket(e));
            }
        }

        /// <summary>
        /// Reads an event from binlog stream.
        /// </summary>
        /// <returns>Binlog event instance. Null if there are no more events</returns>
        public async Task<IBinlogEvent> ReadEventAsync()
        {
            await _channel.Reader.WaitToReadAsync();

            if (!_channel.Reader.TryRead(out IPacket packet))
                return null;

            if (packet is ExceptionPacket exceptionPacket)
                throw new Exception("BinlogReader exception.", exceptionPacket.Exception);

            return packet as IBinlogEvent;
        }
    }
}
