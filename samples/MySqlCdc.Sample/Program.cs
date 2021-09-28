using System.Threading.Tasks;

namespace MySqlCdc.Sample;

class Program
{
    static async Task Main(string[] args)
    {
        await BinlogReaderExample.Start(mariadb: false);
        await BinlogClientExample.Start();
    }
}