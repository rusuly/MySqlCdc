using MySqlCdc.Events;
using MySqlCdc.Providers.MariaDb;
using MySqlCdc.Providers.MySql;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MySqlCdc.Sample;

class BinlogReaderExample
{
    internal static async Task Start(bool mariadb)
    {
        using (Stream stream = File.OpenRead("mysql-bin.000001"))
        {
            EventDeserializer deserializer = mariadb
                ? new MariaDbEventDeserializer()
                : new MySqlEventDeserializer();

            var reader = new BinlogReader(deserializer, stream);

            await foreach (var binlogEvent in reader.ReadEvents())
            {
                await PrintEventAsync(binlogEvent);
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