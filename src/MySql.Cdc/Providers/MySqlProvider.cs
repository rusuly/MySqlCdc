using System.Threading.Tasks;
using MySql.Cdc.Commands;
using MySql.Cdc.Events;
using MySql.Cdc.Network;
using MySql.Cdc.Protocol;
using MySql.Cdc.Providers.MySql;

namespace MySql.Cdc.Providers
{
    public class MySqlProvider : IDatabaseProvider
    {
        public EventDeserializer Deserializer { get; } = new MySqlEventDeserializer();

        public async Task DumpBinlogAsync(DatabaseConnection channel, ConnectionOptions options)
        {
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
