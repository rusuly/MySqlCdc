using System;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Commands;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Packets;
using MySqlCdc.Providers.MariaDb;

namespace MySqlCdc.Providers
{
    internal class MariaDbProvider : IDatabaseProvider
    {
        public EventDeserializer Deserializer { get; } = new MariaDbEventDeserializer();

        public async Task DumpBinlogAsync(DatabaseConnection channel, ConnectionOptions options, CancellationToken cancellationToken = default)
        {
            await channel.WriteCommandAsync(new QueryCommand("SET @mariadb_slave_capability=4"), 0, cancellationToken);
            var packet = await channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, $"Setting @mariadb_slave_capability error.");

            if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
            {
                await RegisterGtidSlave(channel, options, cancellationToken);
            }

            long serverId = options.Blocking ? options.ServerId : 0;
            var command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
            await channel.WriteCommandAsync(command, 0, cancellationToken);
        }

        private async Task RegisterGtidSlave(DatabaseConnection channel, ConnectionOptions options, CancellationToken cancellationToken = default)
        {
            var gtidList = (GtidList)options.Binlog.GtidState;
            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_connect_state='{gtidList.ToString()}'"), 0, cancellationToken);
            var packet = await channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, $"Setting @slave_connect_state error.");

            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_strict_mode=0"), 0, cancellationToken);
            packet = await channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, $"Setting @slave_gtid_strict_mode error.");

            await channel.WriteCommandAsync(new QueryCommand($"SET @slave_gtid_ignore_duplicates=0"), 0, cancellationToken);
            packet = await channel.ReadPacketSlowAsync(cancellationToken);
            ThrowIfErrorPacket(packet, $"Setting @slave_gtid_ignore_duplicates error.");

            await channel.WriteCommandAsync(new RegisterSlaveCommand(options.ServerId), 0, cancellationToken);
            packet = await channel.ReadPacketSlowAsync(cancellationToken);
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