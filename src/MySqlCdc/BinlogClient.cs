using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Checksum;
using MySqlCdc.Commands;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Packets;
using MySqlCdc.Protocol;
using MySqlCdc.Providers;

namespace MySqlCdc
{
    /// <summary>
    /// MySql replication client streaming binlog events in real-time.
    /// </summary>
    public class BinlogClient
    {
        private readonly List<string> _allowedAuthPlugins = new()
        {
            AuthPluginNames.MySqlNativePassword,
            AuthPluginNames.CachingSha2Password
        };

        private readonly ConnectionOptions _options = new();
        private IDatabaseProvider _databaseProvider = default!;
        private DatabaseConnection _channel = default!;

        private IGtid? _gtid;
        private bool _transaction;

        /// <summary>
        /// Creates a new <see cref="BinlogClient"/>.
        /// </summary>
        /// <param name="configureOptions">The configure callback</param>
        public BinlogClient(Action<ConnectionOptions> configureOptions)
        {
            configureOptions(_options);
        }

        /// <summary>
        /// Gets current replication state.
        /// </summary>
        public BinlogOptions State => _options.Binlog;

        private async Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            _channel = new DatabaseConnection(_options);

            var packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, "Initial handshake error.");
            var handshake = new HandshakePacket(new ReadOnlySequence<byte>(packet));

            if (!_allowedAuthPlugins.Contains(handshake.AuthPluginName))
                throw new InvalidOperationException($"Authentication plugin {handshake.AuthPluginName} is not supported.");

            _databaseProvider = handshake.ServerVersion.Contains("MariaDB") ? new MariaDbProvider() : new MySqlProvider();
            await AuthenticateAsync(handshake, cancellationToken);
        }

        private async Task AuthenticateAsync(HandshakePacket handshake, CancellationToken cancellationToken = default)
        {
            byte sequenceNumber = 1;

            bool useSsl = false;
            if (_options.SslMode != SslMode.DISABLED)
            {
                bool sslAvailable = (handshake.ServerCapabilities & (long)CapabilityFlags.SSL) != 0;

                if (!sslAvailable && _options.SslMode >= SslMode.REQUIRE)
                    throw new InvalidOperationException("The server doesn't support SSL encryption");

                if (sslAvailable)
                {
                    var command = new SslRequestCommand(PacketConstants.Utf8Mb4GeneralCi);
                    await _channel.WriteCommandAsync(command, sequenceNumber++, cancellationToken);
                    _channel.UpgradeToSsl();
                    useSsl = true;
                }
            }

            var authenticateCommand = new AuthenticateCommand(_options, PacketConstants.Utf8Mb4GeneralCi, handshake.Scramble, handshake.AuthPluginName);
            await _channel.WriteCommandAsync(authenticateCommand, sequenceNumber, cancellationToken);
            var packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            sequenceNumber += 2;

            ThrowIfErrorPacket(packet, "Authentication error.");

            if (packet[0] == (byte)ResponseType.Ok)
                return;

            if (packet[0] == (byte)ResponseType.AuthPluginSwitch)
            {
                var body = new ReadOnlySequence<byte>(packet, 1, packet.Length - 1);
                var switchRequest = new AuthPluginSwitchPacket(body);
                await HandleAuthPluginSwitch(switchRequest, sequenceNumber, useSsl, cancellationToken);
            }
            else
            {
                await AuthenticateSha256Async(packet, handshake.Scramble, sequenceNumber, useSsl, cancellationToken);
            }
        }

        private async Task HandleAuthPluginSwitch(AuthPluginSwitchPacket switchRequest, byte sequenceNumber, bool useSsl, CancellationToken cancellationToken = default)
        {
            if (!_allowedAuthPlugins.Contains(switchRequest.AuthPluginName))
                throw new InvalidOperationException($"Authentication plugin {switchRequest.AuthPluginName} is not supported.");

            var switchCommand = new AuthPluginSwitchCommand(_options.Password, switchRequest.AuthPluginData, switchRequest.AuthPluginName);
            await _channel.WriteCommandAsync(switchCommand, sequenceNumber, cancellationToken);
            var packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            sequenceNumber += 2;
            ThrowIfErrorPacket(packet, "Authentication switch error.");

            if (switchRequest.AuthPluginName == AuthPluginNames.CachingSha2Password)
            {
                await AuthenticateSha256Async(packet, switchRequest.AuthPluginData, sequenceNumber, useSsl, cancellationToken);
            }
        }

        private async Task AuthenticateSha256Async(byte[] packet, string scramble, byte sequenceNumber, bool useSsl, CancellationToken cancellationToken = default)
        {
            // See https://mariadb.com/kb/en/caching_sha2_password-authentication-plugin/
            // Success authentication.
            if (packet[0] == 0x01 && packet[1] == 0x03)
                return;

            // Send clear password if ssl is used.
            var writer = new PacketWriter(sequenceNumber);
            if (useSsl)
            {
                writer.WriteNullTerminatedString(_options.Password);
                await _channel.WriteBytesAsync(writer.CreatePacket(), cancellationToken);
                packet = await _channel.ReadPacketSlowAsync(cancellationToken);
                ThrowIfErrorPacket(packet, "Sending caching_sha2_password clear password error.");
                return;
            }

            // Request public key.
            writer = new PacketWriter(sequenceNumber);
            writer.WriteByte(0x02);
            await _channel.WriteBytesAsync(writer.CreatePacket(), cancellationToken);
            packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            sequenceNumber += 2;
            ThrowIfErrorPacket(packet, "Requesting caching_sha2_password public key.");

            // Extract public key.
            var publicKey = Encoding.UTF8.GetString(new ReadOnlySequence<byte>(packet, 1, packet.Length - 1).ToArray());

            // Password must be null terminated. Not documented in MariaDB.
            var password = Encoding.UTF8.GetBytes(_options.Password += '\0');
            var encryptedPassword = AuthenticateCommand.Xor(password, Encoding.UTF8.GetBytes(scramble));

            using (var rsa = RSA.Create())
            {
                rsa.ImportFromPem(publicKey);
                var encryptedBody = rsa.Encrypt(encryptedPassword, RSAEncryptionPadding.OaepSHA1);

                writer = new PacketWriter(sequenceNumber);
                writer.WriteByteArray(encryptedBody);
                await _channel.WriteBytesAsync(writer.CreatePacket(), cancellationToken);
                packet = await _channel.ReadPacketSlowAsync(cancellationToken);
                ThrowIfErrorPacket(packet, "Authentication error.");
            }
        }

        /// <summary>
        /// Replicates binlog events from the server
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task completed when last event is read in non-blocking mode</returns>
        public async IAsyncEnumerable<IBinlogEvent> Replicate([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_options.SslMode == SslMode.REQUIRE_VERIFY_CA || _options.SslMode == SslMode.REQUIRE_VERIFY_FULL)
                throw new NotSupportedException($"{nameof(SslMode.REQUIRE_VERIFY_CA)} and {nameof(SslMode.REQUIRE_VERIFY_FULL)} ssl modes are not supported");

            await ConnectAsync(cancellationToken);

            // Clear on reconnect
            _gtid = null;
            _transaction = false;

            await AdjustStartingPosition(cancellationToken);
            await SetMasterHeartbeat(cancellationToken);
            await SetMasterBinlogChecksum(cancellationToken);
            await _databaseProvider.DumpBinlogAsync(_channel, _options, cancellationToken);

            var eventStreamReader = new EventStreamReader(_databaseProvider.Deserializer);
            var channel = new EventStreamChannel(eventStreamReader, _channel.Stream);
            var timeout = _options.HeartbeatInterval.Add(TimeSpan.FromMilliseconds(TimeoutConstants.Delta));

            await foreach (var packet in channel.ReadPacketAsync(timeout, cancellationToken).WithCancellation(cancellationToken))
            {
                if (packet is IBinlogEvent binlogEvent)
                {
                    // We stop replication if client code throws an exception
                    // As a derived database may end up in an inconsistent state.
                    yield return binlogEvent;

                    // Commit replication state if there is no exception.
                    UpdateGtidPosition(binlogEvent);
                    UpdateBinlogPosition(binlogEvent);
                }
                else if (packet is EndOfFilePacket && !_options.Blocking)
                    yield break;
                else if (packet is ErrorPacket error)
                    throw new InvalidOperationException($"Event stream error. {error.ToString()}");
                else
                    throw new InvalidOperationException($"Event stream unexpected error.");
            }
        }

        private void UpdateGtidPosition(IBinlogEvent binlogEvent)
        {
            if (_options.Binlog.StartingStrategy != StartingStrategy.FromGtid)
                return;

            if (binlogEvent is GtidEvent gtidEvent)
            {
                _gtid = gtidEvent.Gtid;
            }
            else if (binlogEvent is XidEvent xidEvent)
            {
                CommitGtid();
            }
            else if (binlogEvent is QueryEvent queryEvent)
            {
                if (string.IsNullOrWhiteSpace(queryEvent.SqlStatement))
                    return;

                if (queryEvent.SqlStatement == "BEGIN")
                {
                    _transaction = true;
                }
                else if (queryEvent.SqlStatement == "COMMIT" || queryEvent.SqlStatement == "ROLLBACK")
                {
                    CommitGtid();
                }
                else if (!_transaction)
                {
                    // Auto-commit query like DDL
                    CommitGtid();
                }
            }
        }

        private void CommitGtid()
        {
            _transaction = false;
            if (_gtid != null)
                _options.Binlog.GtidState!.AddGtid(_gtid);
        }

        private void UpdateBinlogPosition(IBinlogEvent binlogEvent)
        {
            // Rows event depends on preceding TableMapEvent & we change the position
            // after we read them atomically to prevent missing mapping on reconnect.
            // Figure out something better as TableMapEvent can be followed by several row events.
            if (binlogEvent is TableMapEvent tableMapEvent)
                return;

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

        private async Task AdjustStartingPosition(CancellationToken cancellationToken = default)
        {
            if (_options.Binlog.StartingStrategy != StartingStrategy.FromEnd)
                return;

            // Ignore if position was read before in case of reconnect.
            if (!string.IsNullOrWhiteSpace(_options.Binlog.Filename))
                return;

            var command = new QueryCommand("show master status");
            await _channel.WriteCommandAsync(command, 0, cancellationToken);

            var resultSet = await ReadResultSet(cancellationToken);
            if (resultSet.Count != 1)
                throw new InvalidOperationException("Could not read master binlog position.");

            _options.Binlog.Filename = resultSet[0].Cells[0];
            _options.Binlog.Position = long.Parse(resultSet[0].Cells[1]);
        }

        private async Task SetMasterHeartbeat(CancellationToken cancellationToken = default)
        {
            long milliseconds = (long)_options.HeartbeatInterval.TotalMilliseconds;
            long nanoseconds = milliseconds * 1000 * 1000;
            var command = new QueryCommand($"set @master_heartbeat_period={nanoseconds}");
            await _channel.WriteCommandAsync(command, 0, cancellationToken);
            var packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, "Setting master_binlog_checksum error.");
        }

        private async Task SetMasterBinlogChecksum(CancellationToken cancellationToken = default)
        {
            var command = new QueryCommand("SET @master_binlog_checksum= @@global.binlog_checksum");
            await _channel.WriteCommandAsync(command, 0, cancellationToken);
            var packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, "Setting master_binlog_checksum error.");

            command = new QueryCommand($"SELECT @master_binlog_checksum");
            await _channel.WriteCommandAsync(command, 0, cancellationToken);
            var resultSet = await ReadResultSet(cancellationToken);

            // When replication is started fake RotateEvent comes before FormatDescriptionEvent.
            // In order to deserialize the event we have to obtain checksum type length in advance.
            var checksumType = (ChecksumType)Enum.Parse(typeof(ChecksumType), resultSet[0].Cells[0]);
            _databaseProvider.Deserializer.ChecksumStrategy = checksumType switch
            {
                ChecksumType.NONE => new NoneChecksum(),
                ChecksumType.CRC32 => new Crc32Checksum(),
                _ => throw new InvalidOperationException("The master checksum type is not supported.")
            };
        }

        private async Task<List<ResultSetRowPacket>> ReadResultSet(CancellationToken cancellationToken = default)
        {
            var packet = await _channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, "Reading result set error.");

            while (!cancellationToken.IsCancellationRequested)
            {
                // Skip through metadata
                packet = await _channel.ReadPacketSlowAsync(cancellationToken);
                if (packet[0] == (byte)ResponseType.EndOfFile)
                    break;
            }

            var resultSet = new List<ResultSetRowPacket>();
            while (!cancellationToken.IsCancellationRequested)
            {
                packet = await _channel.ReadPacketSlowAsync(cancellationToken);
                ThrowIfErrorPacket(packet, "Query result set error.");

                if (packet[0] == (byte)ResponseType.EndOfFile)
                    break;

                resultSet.Add(new ResultSetRowPacket(new ReadOnlySequence<byte>(packet)));
            }
            return resultSet;
        }

        private void ThrowIfErrorPacket(byte[] packet, string message)
        {
            if (packet[0] == (byte)ResponseType.Error)
            {
                var error = new ErrorPacket(new ReadOnlySequence<byte>(packet, 1, packet.Length - 1));
                throw new InvalidOperationException($"{message} {error.ToString()}");
            }
        }
    }
}