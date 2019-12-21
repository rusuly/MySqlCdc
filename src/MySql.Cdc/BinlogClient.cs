using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Cdc.Checksum;
using MySql.Cdc.Commands;
using MySql.Cdc.Constants;
using MySql.Cdc.Events;
using MySql.Cdc.Network;
using MySql.Cdc.Packets;
using MySql.Cdc.Protocol;
using MySql.Cdc.Providers;

namespace MySql.Cdc
{
    /// <summary>
    /// MySql replication client streaming binlog events in real-time.
    /// </summary>
    public class BinlogClient
    {
        private readonly ConnectionOptions _options = new ConnectionOptions();
        private readonly EventDeserializer _eventDeserializer = new EventDeserializer();
        private readonly IDatabaseProvider _databaseProvider = new MariaDbProvider();
        private PacketChannel _channel;

        public BinlogClient(Action<ConnectionOptions> configureOptions)
        {
            configureOptions(_options);
        }

        private async Task ConnectAsync()
        {
            _channel = new PacketChannel(_options, _eventDeserializer);
            var handshake = await ReceiveHandshakeAsync();
            await AuthenticateAsync(handshake);
        }

        private async Task<HandshakePacket> ReceiveHandshakeAsync()
        {
            var packet = await _channel.ReadPacketAsync();

            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Initial handshake error. {error.ToString()}");

            return packet as HandshakePacket;
        }

        private async Task AuthenticateAsync(HandshakePacket handshake)
        {
            byte sequenceNumber = 1;

            if (_options.UseSsl)
            {
                await SendSslRequest(handshake, sequenceNumber++);
            }

            var authenticateCommand = new AuthenticateCommand(_options, handshake.ServerCollation, handshake.Scramble);
            await _channel.WriteCommandAsync(authenticateCommand, sequenceNumber++);
            var packet = await _channel.ReadPacketAsync();

            if (packet is OkPacket)
                return;

            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Authentication error. {error.ToString()}");

            if (packet is AuthPluginSwitchPacket switchRequest)
            {
                await HandleAuthPluginSwitch(switchRequest, sequenceNumber++);
                return;
            }
            throw new InvalidOperationException($"Authentication error. Unknown authentication switch request header.");
        }

        private async Task SendSslRequest(HandshakePacket handshake, byte sequenceNumber)
        {
            //TODO: Implement SSL/TLS
            throw new NotImplementedException();
        }

        private async Task HandleAuthPluginSwitch(AuthPluginSwitchPacket switchRequest, byte sequenceNumber)
        {
            if (switchRequest.AuthPluginName != AuthPluginNames.MySqlNativePassword)
                throw new InvalidOperationException($"Authentication switch error. {switchRequest.AuthPluginName} plugin is not supported.");

            var switchCommand = new MySqlNativePasswordPluginCommand(_options.Password, switchRequest.AuthPluginData);
            await _channel.WriteCommandAsync(switchCommand, sequenceNumber);
            var packet = await _channel.ReadPacketAsync();

            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Authentication switch error. {error.ToString()}");
        }

        public async Task ReplicateAsync(Func<IBinlogEvent, Task> handleEvent)
        {
            await ConnectAsync();

            await AdjustStartingPosition();
            await SetMasterHeartbeat();
            await SetMasterBinlogChecksum();
            await _databaseProvider.PrepareAsync(_channel);

            _channel.SwitchToStream();

            long serverId = _options.Blocking ? _options.ServerId : 0;
            var command = new DumpBinlogCommand(serverId, _options.Binlog.Filename, _options.Binlog.Position);
            await _channel.WriteCommandAsync(command, 0);

            await ReadEventStreamAsync(handleEvent);
        }

        private async Task ReadEventStreamAsync(Func<IBinlogEvent, Task> handleEvent)
        {
            while (true)
            {
                var packet = await _channel.ReadPacketAsync();

                if (packet is IBinlogEvent binlogEvent)
                {
                    // We stop replication if client code throws an exception 
                    // As a derived database may end up in an inconsistent state.
                    await handleEvent(binlogEvent);
                    UpdateBinlogPosition(binlogEvent);
                }

                else if (packet is ErrorPacket error)
                    throw new InvalidOperationException($"Event stream error. {error.ToString()}");

                else if (packet is EndOfFilePacket && !_options.Blocking)
                    return;

                else throw new InvalidOperationException($"Event stream unexpected error.");
            }
        }

        private void UpdateBinlogPosition(IBinlogEvent binlogEvent)
        {
            if (binlogEvent is RotateEvent rotateEvent)
            {
                _options.Binlog.Filename = rotateEvent.BinlogFilename;
                _options.Binlog.Position = rotateEvent.BinlogPosition;
            }
            else if (binlogEvent.Header.NextEventPosition > 0)
            {
                _options.Binlog.Position = binlogEvent.Header.NextEventPosition;
            }
        }

        private async Task AdjustStartingPosition()
        {
            if (_options.Binlog.StartingStrategy != StartingStrategy.FromEnd)
                return;

            // Ignore if position was read before in case of reconnect.
            if (_options.Binlog.Filename != null)
                return;

            var command = new QueryCommand("show master status");
            await _channel.WriteCommandAsync(command, 0);

            var resultSet = await ReadResultSet();
            if (resultSet.Count != 1)
                throw new InvalidOperationException("Could not read master binlog position.");

            _options.Binlog.Filename = resultSet[0].Cells[0];
            _options.Binlog.Position = long.Parse(resultSet[0].Cells[1]);
        }

        private async Task SetMasterHeartbeat()
        {
            var seconds = (long)_options.HeartbeatInterval.TotalSeconds;
            var command = new QueryCommand($"set @master_heartbeat_period={seconds * 1000 * 1000 * 1000}");
            await _channel.WriteCommandAsync(command, 0);
            var packet = await _channel.ReadPacketAsync();

            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Setting master_binlog_checksum error. {error.ToString()}");
        }

        private async Task SetMasterBinlogChecksum()
        {
            var command = new QueryCommand("SET @master_binlog_checksum= @@global.binlog_checksum");
            await _channel.WriteCommandAsync(command, 0);
            var packet = await _channel.ReadPacketAsync();

            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Setting master_binlog_checksum error. {error.ToString()}");

            command = new QueryCommand($"SELECT @master_binlog_checksum");
            await _channel.WriteCommandAsync(command, 0);
            var resultSet = await ReadResultSet();

            // When replication is started fake RotateEvent comes before FormatDescriptionEvent.
            // In order to deserialize the event we have to obtain checksum type length in advance.
            var checksumType = (ChecksumType)Enum.Parse(typeof(ChecksumType), resultSet[0].Cells[0]);
            _eventDeserializer.ChecksumStrategy = checksumType switch
            {
                ChecksumType.None => new NoneChecksum(),
                ChecksumType.CRC32 => new Crc32Checksum(),
                _ => throw new InvalidOperationException("The master checksum type is not supported.")
            };
        }

        private async Task<List<ResultSetRowPacket>> ReadResultSet()
        {
            var resultSet = new List<ResultSetRowPacket>();
            IPacket packet = null;
            while (true)
            {
                packet = await _channel.ReadPacketAsync();

                if (packet is ErrorPacket error)
                    throw new InvalidOperationException($"Query result set error. {error.ToString()}");

                if (packet is ResultSetRowPacket row)
                {
                    resultSet.Add(row);
                }
                else return resultSet;
            }
        }
    }
}
