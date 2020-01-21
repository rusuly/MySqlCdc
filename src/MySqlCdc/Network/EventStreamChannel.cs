using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using MySqlCdc.Constants;
using MySqlCdc.Packets;
using MySqlCdc.Protocol;

namespace MySqlCdc.Network
{
    internal class EventStreamChannel
    {
        private readonly Channel<IPacket> _channel = Channel.CreateBounded<IPacket>(
            new BoundedChannelOptions(100)
            {
                SingleReader = true,
                SingleWriter = true
            });

        private readonly IEventStreamReader _eventStreamReader;
        private readonly PipeReader _pipeReader;
        private List<PacketSegment> _multipacket;

        public EventStreamChannel(IEventStreamReader eventStreamReader, Stream stream)
        {
            _eventStreamReader = eventStreamReader;
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
                    // We can't calculate packet size without the packet header
                    if (buffer.Length < PacketConstants.HeaderSize)
                        break;

                    // Make sure the packet fits in the buffer
                    // See: https://mariadb.com/kb/en/library/0-packet/
                    var bodySize = GetBodySize(buffer);
                    var packetSize = PacketConstants.HeaderSize + bodySize;

                    if (buffer.Length < packetSize)
                        break;

                    // Process packet and repeat in case there are more packets in the buffer
                    await OnReceivePacket(buffer.Slice(PacketConstants.HeaderSize, bodySize));
                    buffer = buffer.Slice(buffer.GetPosition(packetSize));
                }

                reader.AdvanceTo(buffer.Start, buffer.End);

                if (result.IsCompleted)
                    break;
            }

            await reader.CompleteAsync();
        }

        private int GetBodySize(ReadOnlySequence<byte> buffer)
        {
            var packetReader = new PacketReader(buffer.Slice(0, PacketConstants.HeaderSize));
            var bodySize = packetReader.ReadInt(3);
            return bodySize;
        }

        private async ValueTask OnReceivePacket(ReadOnlySequence<byte> buffer)
        {
            if (_multipacket != null || buffer.Length == PacketConstants.MaxBodyLength)
            {
                var array = new byte[buffer.Length];
                buffer.CopyTo(array);

                if (_multipacket == null)
                {
                    _multipacket = new List<PacketSegment> { new PacketSegment(array) };
                }
                else
                {
                    var lastNode = _multipacket.Last();
                    _multipacket.Add(lastNode.Add(array));
                }

                if (buffer.Length == PacketConstants.MaxBodyLength)
                    return;

                var firstSegment = _multipacket.First();
                var lastSegment = _multipacket.Last();
                buffer = new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
                _multipacket = null;
            }

            var packet = _eventStreamReader.ReadPacket(buffer);
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
