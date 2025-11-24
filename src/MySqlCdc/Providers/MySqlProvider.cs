using MySqlCdc.Commands;
using MySqlCdc.Events;
using MySqlCdc.Network;
using MySqlCdc.Protocol;
using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Providers;

internal class MySqlProvider : IDatabaseProvider
{
    public EventDeserializer Deserializer { get; } = new MySqlEventDeserializer();
    public string ServerVersion { get; }

    public MySqlProvider(string serverVersion)
    {
        ServerVersion = serverVersion;
    }

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

    public Task ShowBinlogStatus(Connection channel, CancellationToken cancellationToken = default)
    {
        QueryCommand command;

        // If version is less than 8.4.0, use the SHOW MASTER STATUS command to get the starting position.
        // Otherwise, use the SHOW BINARY LOG STATUS commands to get the starting position.
        var versionNumbers = ServerVersion.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var majorVersion = int.Parse(versionNumbers[0]);
        if (majorVersion < 8 ||
            (majorVersion == 8 && int.Parse(versionNumbers[1]) < 4))
        {
            command = new QueryCommand("SHOW MASTER STATUS");
        }
        else
        {
            command = new QueryCommand("SHOW BINARY LOG STATUS");
        }

        return channel.WritePacketAsync(command.Serialize(), 0, cancellationToken);
    }
}