using MySqlCdc.Commands;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Protocol;
using MySqlCdc.Providers.MariaDb;

namespace MySqlCdc.Providers;

internal class MariaDbProvider : IDatabaseProvider
{
    public EventDeserializer Deserializer { get; } = new MariaDbEventDeserializer();

    public async Task DumpBinlogAsync(Connection channel, ReplicaOptions options, CancellationToken cancellationToken = default)
    {
        ICommand command = new QueryCommand("SET @mariadb_slave_capability=4");
        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        var (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @mariadb_slave_capability error.");

        if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
        {
            await RegisterGtidSlave(channel, options, cancellationToken);
        }

        long serverId = options.Blocking ? options.ServerId : 0;
        command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
    }

    private async Task RegisterGtidSlave(Connection channel, ReplicaOptions options, CancellationToken cancellationToken = default)
    {
        var gtidList = (GtidList)options.Binlog.GtidState!;
        ICommand command = new QueryCommand($"SET @slave_connect_state='{gtidList.ToString()}'");
        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        var (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @slave_connect_state error.");

        command = new QueryCommand($"SET @slave_gtid_strict_mode=0");
        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @slave_gtid_strict_mode error.");

        command = new QueryCommand($"SET @slave_gtid_ignore_duplicates=0");
        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @slave_gtid_ignore_duplicates error.");

        command = new RegisterSlaveCommand(options.ServerId);
        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
        (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Registering slave error.");
    }
}