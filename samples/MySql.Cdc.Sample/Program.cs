using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySql.Cdc.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
                options.HeartbeatInterval = TimeSpan.FromSeconds(10);
                options.Blocking = true;
                options.Binlog = BinlogOptions.FromStart();
            });

            await client.ReplicateAsync(async (binlogEvent) =>
            {
                await PrintEventAsync(binlogEvent);
            });
        }

        private static async Task PrintEventAsync(IBinlogEvent binlogEvent)
        {
            var json = JsonConvert.SerializeObject(binlogEvent, Formatting.Indented,
            new Newtonsoft.Json.JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            });
            await Console.Out.WriteLineAsync(json);
        }
    }
}
