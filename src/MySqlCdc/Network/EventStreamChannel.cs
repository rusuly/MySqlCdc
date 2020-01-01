using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Channels;
using System.Threading.Tasks;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Packets;
using MySqlCdc.Protocol;
using Pipelines.Sockets.Unofficial;

namespace MySqlCdc.Network
{
    public class EventStreamChannel
    {
        private readonly Channel<IPacket> _channel = Channel.CreateBounded<IPacket>(
            new BoundedChannelOptions(100)
            {
                SingleReader = true,
                SingleWriter = true
            });

        private readonly EventDeserializer _eventDeserializer;
        private readonly IDuplexPipe _duplexPipe;

        public EventStreamChannel(EventDeserializer eventDeserializer, Stream stream)
        {
            _eventDeserializer = eventDeserializer;
            _duplexPipe = StreamConnection.GetDuplex(stream);
            _ = Task.Run(async () => await ReceivePacketsAsync(_duplexPipe.Input));
        }

        private async Task ReceivePacketsAsync(PipeReader reader)
        {
            while (true)
            {
                ReadResult result = await reader.ReadAsync();
                ReadOnlySequence<byte> buffer = result.Buffer;

                while (true)
                {
                    // We can't calculate packet size without the packet header
                    if (buffer.Length < PacketConstants.HeaderSize)
                        break;

                    // Make sure the packet fits in the buffer
                    // See: https://mariadb.com/kb/en/library/0-packet/
                    var header = buffer.Slice(0, PacketConstants.HeaderSize).ToArray();
                    var bodySize = header[0] + (header[1] << 8) + (header[2] << 16);
                    var packetSize = PacketConstants.HeaderSize + bodySize;

                    // TODO: Implement packet splitting
                    if (bodySize == PacketConstants.MaxBodyLength)
                        throw new Exception("Packet splitting is currently not supported");

                    if (buffer.Length < packetSize)
                        break;

                    // Process packet and repeat in case there are more packets in the buffer
                    await OnReceivePacket(buffer.Slice(0, packetSize));
                    buffer = buffer.Slice(buffer.GetPosition(packetSize));
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                    break;
            }

            await reader.CompleteAsync();
        }

        private async Task OnReceivePacket(ReadOnlySequence<byte> sequence)
        {
            var buffer = sequence.Slice(PacketConstants.HeaderSize);
            try
            {
                await OnReceiveStreamPacket(buffer);
            }
            catch (Exception e)
            {
                // We stop replication if deserialize throws an exception 
                // Since a derived database may end up in an inconsistent state.
                await _channel.Writer.WriteAsync(new ExceptionPacket(e));
            }
        }

        private async Task OnReceiveStreamPacket(ReadOnlySequence<byte> buffer)
        {
            var packetReader = new PacketReader(buffer);
            var status = packetReader.ReadInt(1);
            buffer = buffer.Slice(1);

            // Network stream has 3 possible status types.
            IPacket packet = (ResponseType)status switch
            {
                ResponseType.Error => new ErrorPacket(buffer),
                ResponseType.EndOfFile => new EndOfFilePacket(buffer),
                _ => _eventDeserializer.DeserializeEvent(buffer)
            };
            await _channel.Writer.WriteAsync(packet);
        }

        public async Task<IPacket> ReadPacketAsync()
        {
            var packet = await _channel.Reader.ReadAsync();

            if (packet is ExceptionPacket exceptionPacket)
                throw new Exception("Event stream channel exception.", exceptionPacket.Exception);

            return packet;
        }
    }
}
