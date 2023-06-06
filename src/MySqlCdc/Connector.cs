using System.Security.Cryptography;
using System.Text;
using MySqlCdc.Commands;
using MySqlCdc.Constants;
using MySqlCdc.Network;
using MySqlCdc.Packets;
using MySqlCdc.Providers;

namespace MySqlCdc;

internal class Connector
{
    private readonly ReplicaOptions _options;
    private readonly Connection _connection;
    
    private readonly List<string> _allowedAuthPlugins = new()
    {
        AuthPluginNames.MySqlNativePassword,
        AuthPluginNames.CachingSha2Password
    };

    public Connector(ReplicaOptions options)
    {
        _options = options;
        _connection = new Connection(_options);
    }
    
    public async Task<(Connection, IDatabaseProvider)> ConnectAsync(CancellationToken cancellationToken = default)
    {
        var (packet, seqNum) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Initial handshake error.");
        var handshake = new HandshakePacket(packet);

        CheckAuthPlugin(handshake.AuthPluginName);

        IDatabaseProvider databaseProvider = handshake.ServerVersion.Contains("MariaDB") ? new MariaDbProvider() : new MySqlProvider();
        await AuthenticateAsync(handshake, (byte) (seqNum+1), cancellationToken);
        return (_connection, databaseProvider);
    }

    private async Task AuthenticateAsync(HandshakePacket handshake, byte seqNum, CancellationToken cancellationToken = default)
    {
        bool useSsl = false;
        if (_options.SslMode != SslMode.Disabled)
        {
            bool sslAvailable = (handshake.ServerCapabilities & (long)CapabilityFlags.Ssl) != 0;

            if (!sslAvailable && _options.SslMode >= SslMode.Require)
                throw new InvalidOperationException("The server doesn't support SSL encryption");

            if (sslAvailable)
            {
                var command = new SslRequestCommand(PacketConstants.Utf8Mb4GeneralCi);
                await _connection.WritePacketAsync(command.Serialize(), seqNum++, cancellationToken);
                _connection.UpgradeToSsl();
                useSsl = true;
            }
        }

        var authenticateCommand = new AuthenticateCommand(_options, handshake, PacketConstants.Utf8Mb4GeneralCi);
        await _connection.WritePacketAsync(authenticateCommand.Serialize(), seqNum, cancellationToken);
        (var packet, seqNum) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Authentication error.");

        if (packet[0] == (byte)ResponseType.Ok)
            return;

        if (packet[0] == (byte)ResponseType.AuthPluginSwitch)
        {
            var authSwitchPacket = new AuthPluginSwitchPacket(packet[1..]);
            await HandleAuthPluginSwitch(authSwitchPacket, (byte) (seqNum+1), useSsl, cancellationToken);
        }
        else
        {
            await AuthenticateSha256Async(packet, handshake.Scramble, (byte) (seqNum+1), useSsl, cancellationToken);
        }
    }
    
    private async Task HandleAuthPluginSwitch(AuthPluginSwitchPacket authSwitchPacket, byte seqNum, bool useSsl, CancellationToken cancellationToken = default)
    {
        CheckAuthPlugin(authSwitchPacket.AuthPluginName);

        var authSwitchCommand = new AuthPluginSwitchCommand(_options.Password, authSwitchPacket.AuthPluginData, authSwitchPacket.AuthPluginName);
        await _connection.WritePacketAsync(authSwitchCommand.Serialize(), seqNum, cancellationToken);
        (var packet, seqNum) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Authentication switch error.");

        if (authSwitchPacket.AuthPluginName == AuthPluginNames.CachingSha2Password)
        {
            await AuthenticateSha256Async(packet, authSwitchPacket.AuthPluginData, (byte) (seqNum+1), useSsl, cancellationToken);
        }
    }

    private async Task AuthenticateSha256Async(byte[] packet, string scramble, byte seqNum, bool useSsl,
        CancellationToken cancellationToken = default)
    {
        // See https://mariadb.com/kb/en/caching_sha2_password-authentication-plugin/
        // Success authentication.
        if (packet[0] == 0x01 && packet[1] == 0x03)
            return;

        // Password must be null terminated. Not documented in MariaDB.
        var password = Encoding.UTF8.GetBytes(_options.Password += '\0');

        // Send clear password if ssl is used.
        if (useSsl)
        {
            await _connection.WritePacketAsync(password, seqNum, cancellationToken);
            (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
            Extensions.ThrowIfErrorPacket(packet, "Sending caching_sha2_password clear password error.");
            return;
        }

        // Request public key.
        await _connection.WritePacketAsync(new byte[] {0x02}, seqNum, cancellationToken);
        (packet, seqNum) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Requesting caching_sha2_password public key.");

        // Extract public key.
        var publicKey = Encoding.UTF8.GetString(packet[1..]);
        var encryptedPassword = Extensions.Xor(password, Encoding.UTF8.GetBytes(scramble));

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKey);
        var encryptedBody = rsa.Encrypt(encryptedPassword, RSAEncryptionPadding.OaepSHA1);

        await _connection.WritePacketAsync(encryptedBody, ++seqNum, cancellationToken);
        (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Authentication error.");
    }

    private void CheckAuthPlugin(string authPluginName)
    {
        if (!_allowedAuthPlugins.Contains(authPluginName))
            throw new InvalidOperationException($"Authentication plugin {authPluginName} is not supported.");
    }
}