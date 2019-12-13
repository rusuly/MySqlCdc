using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Constants;
using Pipelines.Sockets.Unofficial;

namespace MySql.Cdc.Network
{
    public class PacketChannel
    {
        private readonly ConnectionOptions _options;
        private readonly IDuplexPipe _duplexPipe;

        public PacketChannel(ConnectionOptions options)
        {
            _options = options;

            var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(new IPEndPoint(IPAddress.Loopback, _options.Port));
            _duplexPipe = SocketConnection.Create(socket);

            Task t = Task.Run(async () => await ReceivePacketsAsync(_duplexPipe.Input));
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
                    // TODO: Implement packet compression
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
            var packetBody = sequence.Slice(PacketConstants.HeaderSize);
            using (var awaiter = PacketAwaiter.GetAwaiter())
            {
                awaiter.Caller.SetResult(packetBody.ToArray()/* TODO: Don't allocate memory */);
                await awaiter.Pusher.Task;
            }
        }

        public async Task<byte[]> ReadPacketAsync()
        {
            using (var awaiter = PacketAwaiter.GetAwaiter())
            {
                var response = await awaiter.Caller.Task;
                PacketAwaiter.ResetAwaiter();
                awaiter.Pusher.SetResult(0);
                return response;
            }
        }

        public async Task WritePacketAsync(ICommand command, byte sequenceNumber)
        {
            var array = command.CreatePacket(sequenceNumber);
            await _duplexPipe.Output.WriteAsync(array);
        }
    }
}
