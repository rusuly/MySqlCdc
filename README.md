# MySqlCdc
MySql binlog Change Data Capture (CDC) connector for .NET Core

Acts as MySql replication client streaming binlog events in real-time.

Designed for reactive push-model applications, event sourcing or derived data systems.

NuGet feed: [MySqlCdc](https://www.nuget.org/packages/MySqlCdc)

## Warnings
Be carefull when working with binary log event streaming.
- Binlog stream includes changes made to all databases on the master server including sql queries with sensitive information and you may leak data from the databases. Consider deploying your database to an isolated instance.
- Binlog is an append-only file. It includes changes for databases/tables that you deleted and then recreated. Make sure you don't replay the phantom events in your application.

## Limitations
Please note the lib currently has the following limitations:
- Packet compression is not supported.
- Reading a binlog file offline is not supported.
- Automatic failover is not supported.
- Multi-source replication & multi-master topology setup are not supported.
- Supported auth plugins are `mysql_native_password` and `caching_sha2_password`.
- Currently the lib supports connecting to 'localhost' without SSL encryption.

## Prerequisites
Please make sure the following requirements are met:
1. The user is granted `REPLICATION SLAVE`, `REPLICATION CLIENT` privileges.
2. The following settings must be configured on master server:
```conf
binlog_format=row
binlog_row_image=full
```

## Example
You have to obtain columns ordinal position of the table that you are interested in.
The library is not responsible for this so you have to do it using another tool.
```sql
select TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, ORDINAL_POSITION
from INFORMATION_SCHEMA.COLUMNS
where TABLE_NAME='AspNetUsers' and TABLE_SCHEMA='Identity'
order by ORDINAL_POSITION;
```
Data is stored in Cells property of row events in the same order. See the sample project.
```csharp
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
```
A typical transaction has the following structure.
1. `GtidEvent` if gtid mode is enabled.
2. One or many `TableMapEvent` events.
  - One or many `WriteRowsEvent` events.
  - One or many `UpdateRowsEvent` events.
  - One or many `DeleteRowsEvent` events.
3. `XidEvent` indicating commit of the transaction.

## Type mapping notes

  | MySQL Type         | .NET type            |
  | ------------------ |:--------------------:|
  | DECIMAL            | Not supported ❌     |
  | GEOMETRY           | Not supported ❌     |
  | JSON (MySQL)       | Not supported ❌     |
  | JSON (MariaDB)     | byte[]               |
  | BIT                | BitArray             |
  | TINY (tinyint)     | int                  |
  | SHORT (smallint)   | int                  |
  | INT24 (mediumint)  | int                  |
  | LONG  (int)        | int                  |
  | LONGLONG (bigint)  | long                 |
  | FLOAT (float)      | float                |
  | DOUBLE (double)    | double               |
  | VARCHAR, VARBINARY | string               |
  | CHAR               | string               |
  | ENUM               | int                  |
  | SET                | long                 |
  | YEAR               | int                  |
  | DATE               | Nullable&lt;DateTime&gt; |
  | TIME (old format)  | TimeSpan             |
  | TIMESTAMP          | DateTimeOffset       |
  | DATETIME           | Nullable&lt;DateTime&gt; |
  | TIME2              | TimeSpan             |
  | TIMESTAMP2         | DateTimeOffset       |
  | DATETIME2          | Nullable&lt;DateTime&gt; |
  | BLOB types         | byte[]               |

- Invalid DATE, DATETIME, DATETIME2 values(0000-00-00) are parsed as DateTime null.
- TIME2, DATETIME2, TIMESTAMP2 will loose microseconds when converting to .NET types.
- The lib doesn't distinguish between unsigned and signed. Client should cast to unsigned manually.
- DECIMAL, JSON, GEOMETRY types are not supported now.

## Supported versions
MySqlCdc supports both MariaDB & MySQL server.

  | MariaDB  | Status                   |
  | -------- |:------------------------:|
  | 10.3     | Did not verify           |
  | 10.4     | Supported ✅             |

  | MySQL    | Status                   |
  | -------- |:------------------------:|
  | 5.6      | Did not verify           |
  | 5.7      | Supported ✅             |
  | 8.0      | Supported ✅             |

## Info
The project is based on [mysql-binlog-connector-java](https://github.com/shyiko/mysql-binlog-connector-java) library, MariaDB and MySQL  documentation.

Has a third-party dependency [Pipelines.Sockets.Unofficial](https://github.com/mgravell/Pipelines.Sockets.Unofficial) by Marc Gravell and optimized to use [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines/) as described in his [series of posts](https://blog.marcgravell.com/2018/07/pipe-dreams-part-1.html).

## Q&A
Are uncommited changes written to binlog?
- If you make [transactional changes](https://dev.mysql.com/doc/refman/5.7/en/replication-features-transactions.html) binlog will only include commited transactions in their commit order to provide consistency.
- If you make non-transactional changes binlog will include changes from uncommited transactions even if the transactions are rolled back.

## License
The library is provided under the [MIT License](LICENSE).