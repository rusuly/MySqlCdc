using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Network;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Providers
{
    public class MySqlProvider : IDatabaseProvider
    {
        public async Task DumpBinlogAsync(PacketChannel channel, ConnectionOptions options)
        {
            channel.SwitchToStream();

            long serverId = options.Blocking ? options.ServerId : 0;
            ICommand command = null;

            if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
            {
                command = new DumpBinlogGtidCommand(serverId, options.Binlog.Filename, options.Binlog.Position, options.Binlog.Gtid);
            }
            else
            {
                command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
            }
            
            await channel.WriteCommandAsync(command, 0);
        }
    }
}
