using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Commands;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Providers.MariaDb;

namespace MySqlCdc.Providers;

internal class MariaDbProvider : IDatabaseProvider
{
    public EventDeserializer Deserializer { get; } = new MariaDbEventDeserializer();

    public async Task DumpBinlogAsync(Connection channel, ConnectionOptions options, CancellationToken cancellationToken = default)
    {
        await channel.WriteCommandAsync(new QueryCommand("SET @mariadb_slave_capability=4"), 0, cancellationToken);
        var (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @mariadb_slave_capability error.");

        if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
        {
            await RegisterGtidSlave(channel, options, cancellationToken);
        }

        long serverId = options.Blocking ? options.ServerId : 0;
        var command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
        await channel.WriteCommandAsync(command, 0, cancellationToken);
    }

    private async Task RegisterGtidSlave(Connection channel, ConnectionOptions options, CancellationToken cancellationToken = default)
    {
        var gtidList = (GtidList)options.Binlog.GtidState!;
        await channel.WriteCommandAsync(new QueryCommand($"SET @slave_connect_state='{gtidList.ToString()}'"), 0, cancellationToken);
        var (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @slave_connect_state error.");

        await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_strict_mode=0"), 0, cancellationToken);
        (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @slave_gtid_strict_mode error.");

        await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_ignore_duplicates=0"), 0, cancellationToken);
        (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Setting @slave_gtid_ignore_duplicates error.");

        await channel.WriteCommandAsync(new RegisterSlaveCommand(options.ServerId), 0, cancellationToken);
        (packet, _) = await channel.ReadPacketAsync(cancellationToken);
        Extensions.ThrowIfErrorPacket(packet, $"Registering slave error.");
    }
}