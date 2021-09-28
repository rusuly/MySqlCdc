using System.Threading;
using System.Threading.Tasks;
using MySqlCdc.Events;
using MySqlCdc.Network;

namespace MySqlCdc.Providers;

internal interface IDatabaseProvider
{
    Task DumpBinlogAsync(DatabaseConnection channel, ConnectionOptions options, CancellationToken cancellationToken = default);

    EventDeserializer Deserializer { get; }
}