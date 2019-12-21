using System;
using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Network;
using MySql.Cdc.Packets;

namespace MySql.Cdc.Providers
{
    public class MariaDbProvider : IDatabaseProvider
    {
        public async Task PrepareAsync(PacketChannel channel)
        {
            await SetMariaDbSlaveCapability(channel);
        }

        private async Task SetMariaDbSlaveCapability(PacketChannel channel)
        {
            var command = new QueryCommand("SET @mariadb_slave_capability=4");
            await channel.WriteCommandAsync(command, 0);
            
            var packet = await channel.ReadPacketAsync();
            if (packet is ErrorPacket error)
                throw new InvalidOperationException($"Setting mariadb_slave_capability error. {error.ToString()}");
        }
    }
}
