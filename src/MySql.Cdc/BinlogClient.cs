using System;
using System.Buffers;
using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Constants;
using MySql.Cdc.Network;
using MySql.Cdc.Packets;

namespace MySql.Cdc
{
    /// <summary>
    /// MySql replication client streaming binlog events in real-time.
    /// </summary>
    public class BinlogClient
    {
        private readonly ConnectionOptions _options = new ConnectionOptions();
        private PacketChannel _channel;

        public BinlogClient(Action<ConnectionOptions> configureOptions)
        {
            configureOptions(_options);
        }

        public async Task ConnectAsync()
        {
            _channel = new PacketChannel(_options);
            var handshake = await ReceiveHandshakeAsync();
            await AuthenticateAsync(handshake);
        }

        private async Task<HandshakePacket> ReceiveHandshakeAsync()
        {
            byte[] packet = await _channel.ReadPacketAsync();
            if (packet[0] != (byte)ResponseType.Error)
                return new HandshakePacket(new ReadOnlySequence<byte>(packet));

            var errorPacket = new ErrorPacket(new ReadOnlySequence<byte>(packet));
            throw new InvalidOperationException($"Initial handshake error. {errorPacket.ToString()}");
        }

        private async Task AuthenticateAsync(HandshakePacket handshake)
        {
            byte sequenceNumber = 1;

            if (_options.UseSsl)
            {
                await SendSslRequest(handshake, sequenceNumber++);
            }

            var authenticateCommand = new AuthenticateCommand(_options, handshake.ServerCollation, handshake.Scramble);
            await _channel.WritePacketAsync(authenticateCommand, sequenceNumber++);
            var packet = await _channel.ReadPacketAsync();

            if (packet[0] == (byte)ResponseType.Ok)
                return;

            if (packet[0] == (byte)ResponseType.Error)
            {
                var errorPacket = new ErrorPacket(new ReadOnlySequence<byte>(packet));
                throw new InvalidOperationException($"Authentication error. {errorPacket.ToString()}");
            }
            if (packet[0] == (byte)ResponseType.AuthenticationSwitch)
            {
                var switchRequest = new AuthenticationSwitchPacket(new ReadOnlySequence<byte>(packet));
                await HandleAuthenticationSwitch(switchRequest, sequenceNumber++);
                return;
            }
            throw new InvalidOperationException($"Authentication error. Unknown authentication switch request header: {packet[0]}.");
        }

        private async Task SendSslRequest(HandshakePacket handshake, byte sequenceNumber)
        {
            //TODO: Implement SSL/TLS
            throw new NotImplementedException();
        }

        private async Task HandleAuthenticationSwitch(AuthenticationSwitchPacket switchRequest, byte sequenceNumber)
        {
            if (switchRequest.AuthPluginName != "mysql_native_password")
                throw new InvalidOperationException($"Authentication switch error. {switchRequest.AuthPluginName} plugin is not supported.");

            var switchCommand = new MySqlNativePasswordPluginCommand(_options.Password, switchRequest.AuthPluginData);
            await _channel.WritePacketAsync(switchCommand, sequenceNumber);
            var packet = await _channel.ReadPacketAsync();

            if (packet[0] == (byte)ResponseType.Error)
            {
                var errorPacket = new ErrorPacket(new ReadOnlySequence<byte>(packet));
                throw new InvalidOperationException($"Authentication switch error. {errorPacket.ToString()}");
            }
        }
    }
}
