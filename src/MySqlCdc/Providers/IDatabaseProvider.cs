using MySqlCdc.Events;
using MySqlCdc.Network;

namespace MySqlCdc.Providers;

internal interface IDatabaseProvider
{
    public string ServerVersion { get; }
    Task DumpBinlogAsync(Connection channel, ReplicaOptions options, CancellationToken cancellationToken = default);
    Task ShowBinlogStatus(Connection channel, CancellationToken cancellationToken = default);
    EventDeserializer Deserializer { get; }
}