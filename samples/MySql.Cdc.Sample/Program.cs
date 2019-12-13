using System.Threading.Tasks;

namespace MySql.Cdc.Sample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var client = new BinlogClient(options =>
            {
                options.Port = 3306;
                options.UseSsl = false;
                options.Username = "root";
                options.Password = "Qwertyu1";
            });
            await client.ConnectAsync();
        }
    }
}
