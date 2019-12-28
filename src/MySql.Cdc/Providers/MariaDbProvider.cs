using System;
using System.Buffers;
using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Constants;
using MySql.Cdc.Events;
using MySql.Cdc.Network;
using MySql.Cdc.Packets;
using MySql.Cdc.Providers.MariaDb;

namespace MySql.Cdc.Providers
{
    public class MariaDbProvider : IDatabaseProvider
    {
        public EventDeserializer Deserializer { get; } = new MariaEventDeserializer();

        public async Task DumpBinlogAsync(DatabaseConnection channel, ConnectionOptions options)
        {
            await channel.WriteCommandAsync(new QueryCommand("SET @mariadb_slave_capability=4"), 0);
            var packet = await channel.ReadPacketSlowAsync();
            ThrowIfErrorPacket(packet, $"Setting @mariadb_slave_capability error.");

            if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
            {
                await RegisterGtidSlave(channel, options);
            }

            long serverId = options.Blocking ? options.ServerId : 0;
            var command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
            await channel.WriteCommandAsync(command, 0);
        }

        private async Task RegisterGtidSlave(DatabaseConnection channel, ConnectionOptions options)
        {
            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_connect_state='{options.Binlog.Gtid}'"), 0);
            var packet = await channel.ReadPacketSlowAsync();
            ThrowIfErrorPacket(packet, $"Setting @slave_connect_state error.");

            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_strict_mode=0"), 0);
            packet = await channel.ReadPacketSlowAsync();
            ThrowIfErrorPacket(packet, $"Setting @slave_gtid_strict_mode error.");

            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_ignore_duplicates=0"), 0);
            packet = await channel.ReadPacketSlowAsync();
            ThrowIfErrorPacket(packet, $"Setting @slave_gtid_ignore_duplicates error.");

            await channel.WriteCommandAsync(new RegisterSlaveCommand(options.ServerId), 0);
            packet = await channel.ReadPacketSlowAsync();
            ThrowIfErrorPacket(packet, $"Registering slave error.");
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
