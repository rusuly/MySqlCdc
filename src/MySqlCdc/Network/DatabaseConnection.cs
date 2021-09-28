using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Network;

internal class DatabaseConnection
{
    private readonly ConnectionOptions _options;
    public Stream Stream { get; private set; }

    public DatabaseConnection(ConnectionOptions options)
    {
        _options = options;
        Exception? ex = null;

        var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        foreach (var ip in Dns.GetHostAddresses(options.Hostname))
        {
            try
            {
                socket.Connect(new IPEndPoint(ip, _options.Port));
                Stream = new NetworkStream(socket);
            }
            catch (Exception e)
            {
                ex = e;
            }
        }

        if (Stream == null)
            throw ex ?? new InvalidOperationException("Could not connect to the server");
    }

    public async Task WriteBytesAsync(byte[] array, CancellationToken cancellationToken = default)
    {
        await Stream.WriteAsync(array, 0, array.Length, cancellationToken);
    }

    public async Task WriteCommandAsync(ICommand command, byte sequenceNumber, CancellationToken cancellationToken = default)
    {
        var array = command.CreatePacket(sequenceNumber);
        await Stream.WriteAsync(array, 0, array.Length, cancellationToken);
    }

    /// <summary>
    /// In sequential mode packet type is determined by calling client code.
    /// We don't use System.IO.Pipelines as it cannot determine packet type.
    /// </summary>
    public async Task<byte[]> ReadPacketSlowAsync(CancellationToken cancellationToken = default)
    {
        byte[] header = new byte[PacketConstants.HeaderSize];
        await Stream.ReadAsync(header, 0, header.Length, cancellationToken);

        // We don't care about packet splitting in handshake flow
        var bodySize = header[0] + (header[1] << 8) + (header[2] << 16);
        byte[] body = new byte[bodySize];
        await Stream.ReadAsync(body, 0, body.Length, cancellationToken);

        return body;
    }

    public void UpgradeToSsl()
    {
        var sslStream = new SslStream(Stream, false, ValidateServerCertificate, null);
        sslStream.AuthenticateAsClient(_options.Hostname);
        Stream = sslStream;
    }

    private bool ValidateServerCertificate(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors)
    {
        if (_options.SslMode == SslMode.IF_AVAILABLE || _options.SslMode == SslMode.REQUIRE)
            return true;

        if (sslPolicyErrors == SslPolicyErrors.None)
            return true;

        return false;
    }
}