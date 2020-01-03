using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MySqlCdc.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MySqlCdc.Sample
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
                options.HeartbeatInterval = TimeSpan.FromSeconds(30);
                options.Blocking = true;
                
                // Start replication from MariaDB GTID
                //options.Binlog = BinlogOptions.FromGtid("0-1-270");

                // Start replication from MySQL GTID
                //options.Binlog = BinlogOptions.FromGtid("f442510a-2881-11ea-b1dd-27916133dbb2:1-7");
                
                // Start replication from the position
                //options.Binlog = BinlogOptions.FromPosition("mysql-bin.000008", 195);

                // Start replication from last master position.
                // Useful when you are only interested in new changes.
                //options.Binlog = BinlogOptions.FromEnd();

                // Start replication from first event of first available master binlog.
                // Note that binlog files by default have expiration time and deleted.
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
