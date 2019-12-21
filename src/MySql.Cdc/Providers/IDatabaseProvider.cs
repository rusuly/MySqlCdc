using System.Threading.Tasks;
using MySql.Cdc.Network;

namespace MySql.Cdc.Providers
{
    public interface IDatabaseProvider
    {
        Task PrepareAsync(PacketChannel channel);
    }
}
