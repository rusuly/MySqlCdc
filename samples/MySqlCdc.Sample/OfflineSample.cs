using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MySqlCdc.Events;
using MySqlCdc.Providers.MariaDb;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MySqlCdc.Sample
{
    class OfflineSample
    {
        internal static async Task Start()
        {
            using (FileStream fs = File.OpenRead("mariadb-bin.000002"))
            {
                var reader = new BinlogFileReader(new MariaEventDeserializer(), fs);
                while (true)
                {
                    var @event = await reader.ReadEventAsync();
                    if (@event != null)
                    {
                        await PrintEventAsync(@event);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private static async Task PrintEventAsync(IBinlogEvent binlogEvent)
        {
            var json = JsonConvert.SerializeObject(binlogEvent, Formatting.Indented,
            new JsonSerializerSettings()
            {
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            });
            await Console.Out.WriteLineAsync(json);
        }
    }
}
