using System;
using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Network;
using MySql.Cdc.Packets;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Providers
{
    public class MariaDbProvider : IDatabaseProvider
    {
        public async Task DumpBinlogAsync(PacketChannel channel, ConnectionOptions options)
        {
            await channel.WriteCommandAsync(new QueryCommand("SET @mariadb_slave_capability=4"), 0);
            var packet = await channel.ReadPacketAsync();
            ThrowIfErrorPacket(packet, $"Setting @mariadb_slave_capability error.");

            if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
            {
                await RegisterGtidSlave(channel, options);
            }

            channel.SwitchToStream();

            long serverId = options.Blocking ? options.ServerId : 0;
            var command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
            await channel.WriteCommandAsync(command, 0);
        }

        private async Task RegisterGtidSlave(PacketChannel channel, ConnectionOptions options)
        {
            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_connect_state='{options.Binlog.Gtid}'"), 0);
            var packet = await channel.ReadPacketAsync();
            ThrowIfErrorPacket(packet, $"Setting @slave_connect_state error.");

            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_strict_mode=0"), 0);
            packet = await channel.ReadPacketAsync();
            ThrowIfErrorPacket(packet, $"Setting @slave_gtid_strict_mode error.");

            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_ignore_duplicates=0"), 0);
            packet = await channel.ReadPacketAsync();
            ThrowIfErrorPacket(packet, $"Setting @slave_gtid_ignore_duplicates error.");

            await channel.WriteCommandAsync(new RegisterSlaveCommand(options.ServerId), 0);
            packet = await channel.ReadPacketAsync();
            ThrowIfErrorPacket(packet, $"Registering slave error.");
        }

        private void ThrowIfErrorPacket(IPacket packet, string message)
        {
            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"{message} {error.ToString()}");
        }
    }
}
