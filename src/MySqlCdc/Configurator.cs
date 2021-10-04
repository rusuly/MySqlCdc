using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Checksum;
using MySqlCdc.Commands;
using MySqlCdc.Constants;
using MySqlCdc.Network;
using MySqlCdc.Packets;
using MySqlCdc.Providers;

namespace MySqlCdc;

internal class Configurator
{
    private readonly ConnectionOptions _options;
    private readonly Connection _connection;
    private readonly IDatabaseProvider _databaseProvider;
    
    public Configurator(ConnectionOptions options, Connection connection, IDatabaseProvider databaseProvider)
    {
        _options = options;
        _connection = connection;
        _databaseProvider = databaseProvider;
    }
    
    public async Task AdjustStartingPosition(CancellationToken cancellationToken = default)
    {
        if (_options.Binlog.StartingStrategy != StartingStrategy.FromEnd)
            return;

        // Ignore if position was read before in case of reconnect.
        if (_options.Binlog.Filename != string.Empty)
            return;

        var command = new QueryCommand("show master status");
        await _connection.WritePacketAsync(command.Serialize(), 0, cancellationToken);

        var resultSet = await ReadResultSet(cancellationToken);
        if (resultSet.Count != 1)
            throw new InvalidOperationException("Could not read master binlog position.");

        _options.Binlog.Filename = resultSet[0].Cells[0];
        _options.Binlog.Position = long.Parse(resultSet[0].Cells[1]);
    }
    
    public async Task SetMasterHeartbeat(CancellationToken cancellationToken = default)
    {
        long milliseconds = (long)_options.HeartbeatInterval.TotalMilliseconds;
        long nanoseconds = milliseconds * 1000 * 1000;
        var command = new QueryCommand($"set @master_heartbeat_period={nanoseconds}");
        await _connection.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        var (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Setting master_binlog_checksum error.");
    }

    public async Task SetMasterBinlogChecksum(CancellationToken cancellationToken = default)
    {
        var command = new QueryCommand("SET @master_binlog_checksum= @@global.binlog_checksum");
        await _connection.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        var (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Setting master_binlog_checksum error.");

        command = new QueryCommand($"SELECT @master_binlog_checksum");
        await _connection.WritePacketAsync(command.Serialize(), 0, cancellationToken);
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
        var (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, "Reading result set error.");

        while (!cancellationToken.IsCancellationRequested)
        {
            // Skip through metadata
            (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
            if (packet[0] == (byte)ResponseType.EndOfFile)
                break;
        }

        var resultSet = new List<ResultSetRowPacket>();
        while (!cancellationToken.IsCancellationRequested)
        {
            (packet, _) = await _connection.ReadPacketAsync(cancellationToken);
            Extensions.ThrowIfErrorPacket(packet, "Query result set error.");

            if (packet[0] == (byte)ResponseType.EndOfFile)
                break;

            resultSet.Add(new ResultSetRowPacket(packet));
        }
        return resultSet;
    }
}