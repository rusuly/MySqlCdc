using MySqlCdc.Commands;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Protocol;
using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Providers;

internal class MySqlProvider : IDatabaseProvider
{
    public EventDeserializer Deserializer { get; } = new MySqlEventDeserializer();

    public async Task DumpBinlogAsync(Connection channel, ReplicaOptions options, CancellationToken cancellationToken = default)
    {
        long serverId = options.Blocking ? options.ServerId : 0;
            
        ICommand command;
        if (options.Binlog.StartingStrategy == StartingStrategy.FromGtid)
        {
            var gtidSet = (GtidSet)options.Binlog.GtidState!;
            command = new DumpBinlogGtidCommand(serverId, options.Binlog.Filename, options.Binlog.Position, gtidSet);
        }
        else
        {
            command = new DumpBinlogCommand(serverId, options.Binlog.Filename, options.Binlog.Position);
        }

        await channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
    }
}