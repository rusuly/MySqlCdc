using System.Threading.Tasks;
using MySqlCdc.Events;
using MySqlCdc.Network;

namespace MySqlCdc.Providers
{
    public interface IDatabaseProvider
    {
        Task DumpBinlogAsync(DatabaseConnection channel, ConnectionOptions options);
        EventDeserializer Deserializer { get; }
    }
}
