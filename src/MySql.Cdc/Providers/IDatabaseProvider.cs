using System.Threading.Tasks;
using MySql.Cdc.Events;
using MySql.Cdc.Network;

namespace MySql.Cdc.Providers
{
    public interface IDatabaseProvider
    {
        Task DumpBinlogAsync(DatabaseConnection channel, ConnectionOptions options);
        EventDeserializer Deserializer { get; }
    }
}
