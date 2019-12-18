using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Constants;
using MySql.Cdc.Events;
using MySql.Cdc.Packets;
using MySql.Cdc.Protocol;
using Pipelines.Sockets.Unofficial;

namespace MySql.Cdc.Network
{
    public class PacketChannel
    {
        private static readonly BoundedChannelOptions _channelOptions = new BoundedChannelOptions(100)
        {
            SingleReader = true,
            SingleWriter = true
        };

        private readonly EventDeserializer _eventDeserializer = new EventDeserializer();
        private readonly Channel<IPacket> _channel = Channel.CreateBounded<IPacket>(_channelOptions);
        private readonly ConnectionOptions _options;
        private readonly IDuplexPipe _duplexPipe;
        private bool _streaming = false;
        private int _processed = 0;
        private ICommand _lastCommand = null;
        private int _resultSetPacket = 0;
        private int _resultSetEofNum = 0;

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
            var buffer = sequence.Slice(PacketConstants.HeaderSize);

            if (_streaming)
            {
                await OnReceiveStreamPacket(buffer);
            }
            else
            {
                await OnReceiveSerialPacket(buffer);
            }
        }

        private async Task OnReceiveSerialPacket(ReadOnlySequence<byte> buffer)
        {
            var packetReader = new PacketReader(buffer);
            var status = packetReader.ReadInt(1);

            // We can only identify HandshakePacket if it's the first packet.
            if (_processed++ == 0 && status != (int)ResponseType.Error)
            {
                await _channel.Writer.WriteAsync(new HandshakePacket(buffer));
                return;
            }

            // TODO: Refactor this state machine
            if (_lastCommand is QueryCommand)
            {
                if (_resultSetPacket == 0)
                {
                    if (status == (int)ResponseType.Ok || status == (int)ResponseType.Error)
                    {
                        _lastCommand = null;
                    }
                    else
                    {
                        _resultSetPacket++;
                        return;
                    }
                }
                else
                {
                    if (buffer.Length == 5 && status == (int)ResponseType.EndOfFile)
                    {
                        if (++_resultSetEofNum == 2)
                        {
                            _lastCommand = null;
                            _resultSetPacket = 0;
                            _resultSetEofNum = 0;
                        }
                        else return;
                    }
                    else if (_resultSetEofNum == 1)
                    {
                        await _channel.Writer.WriteAsync(new ResultSetRowPacket(buffer));
                        return;
                    }
                    else return;
                }
            }

            buffer = buffer.Slice(1);
            if (status == (int)ResponseType.Error)
            {
                await _channel.Writer.WriteAsync(new ErrorPacket(buffer));
            }
            else if (status == (int)ResponseType.EndOfFile)
            {
                await _channel.Writer.WriteAsync(new EndOfFilePacket(buffer));
            }
            else if (status == (int)ResponseType.AuthenticationSwitch)
            {
                await _channel.Writer.WriteAsync(new AuthPluginSwitchPacket(buffer));
            }
            else if (status == (int)ResponseType.Ok)
            {
                await _channel.Writer.WriteAsync(new OkPacket(buffer));
            }
            else
            {
                throw new InvalidOperationException($"Unknown response type {status}");
            }
        }

        private async Task OnReceiveStreamPacket(ReadOnlySequence<byte> buffer)
        {
            var packetReader = new PacketReader(buffer);
            var status = packetReader.ReadInt(1);
            buffer = buffer.Slice(1);

            // Streaming mode has 3 possible status types.
            if (status == (int)ResponseType.Error)
            {
                await _channel.Writer.WriteAsync(new ErrorPacket(buffer));
            }
            else if (status == (int)ResponseType.EndOfFile)
            {
                await _channel.Writer.WriteAsync(new EndOfFilePacket(buffer));
            }
            else
            {
                // We stop replication if deserializator throws an exception 
                // As a derived database may end up in an inconsistent state.
                await _channel.Writer.WriteAsync(_eventDeserializer.DeserializeEvent(buffer));
            }
        }

        public ValueTask<IPacket> ReadPacketAsync()
        {
            return _channel.Reader.ReadAsync();
        }

        public async Task WriteCommandAsync(ICommand command, byte sequenceNumber)
        {
            _lastCommand = command;
            var array = command.CreatePacket(sequenceNumber);
            await _duplexPipe.Output.WriteAsync(array);
        }

        public void SwitchToStream()
        {
            _streaming = true;
        }
    }
}
