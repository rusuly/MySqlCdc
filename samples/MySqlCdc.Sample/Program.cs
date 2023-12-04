using MySqlCdc.Sample;

await BinlogReaderExample.Start(mariadb: false);
await BinlogClientExample.Start();
