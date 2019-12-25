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
                if (binlogEvent is TableMapEvent tableMap)
                {
                    await HandleTableMapEvent(tableMap);
                }
                else if (binlogEvent is WriteRowsEvent writeRows)
                {
                    await HandleWriteRowsEvent(writeRows);
                }
                else if (binlogEvent is UpdateRowsEvent updateRows)
                {
                    await HandleUpdateRowsEvent(updateRows);
                }
                else if (binlogEvent is DeleteRowsEvent deleteRows)
                {
                    await HandleDeleteRowsEvent(deleteRows);
                }
                else await PrintEventAsync(binlogEvent);
            });
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

        private static async Task HandleTableMapEvent(TableMapEvent tableMap)
        {
            Console.WriteLine($"Processing {tableMap.DatabaseName}.{tableMap.TableName}");
            await PrintEventAsync(tableMap);
        }

        private static async Task HandleWriteRowsEvent(WriteRowsEvent writeRows)
        {
            Console.WriteLine($"{writeRows.Rows.Count} rows were written");
            await PrintEventAsync(writeRows);

            foreach (var row in writeRows.Rows)
            {
                // Do something
            }
        }

        private static async Task HandleUpdateRowsEvent(UpdateRowsEvent updatedRows)
        {
            Console.WriteLine($"{updatedRows.Rows.Count} rows were updated");
            await PrintEventAsync(updatedRows);

            foreach (var row in updatedRows.Rows)
            {
                var rowBeforeUpdate = row.BeforeUpdate;
                var rowAfterUpdate = row.AfterUpdate;
                // Do something
            }
        }

        private static async Task HandleDeleteRowsEvent(DeleteRowsEvent deleteRows)
        {
            Console.WriteLine($"{deleteRows.Rows.Count} rows were deleted");
            await PrintEventAsync(deleteRows);

            foreach (var row in deleteRows.Rows)
            {
                // Do something
            }
        }
    }
}
