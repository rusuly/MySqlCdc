# MySqlCdc
MySQL/MariaDB binlog change data capture (CDC) connector for .NET

Acts as a replication client streaming binary log events in real-time.

Designed for reactive push-model applications, event sourcing or derived data systems.

Before integrating MySqlCdc in your project consider using Kafka. It can be more suitable for your architecture.

NuGet feed: [MySqlCdc](https://www.nuget.org/packages/MySqlCdc)

## Use cases
Transaction log events are immutable and appended in strictly sequential order. This simplifies your concurrency model and allows you to avoid distributed locks that handle race conditions from parallel database requests.
- Event sourcing.
- Cache invalidation.
- OLAP. Analytics. Reporting. Data Warehouse.
- Real-time chat/messenger using web sockets.
- Synchronizing web/mobile client state with backend.
- Replicating MySQL database to Memcached/Redis cache. 
- Replicating MySQL database to NoSQL/Elasticsearch. Denormalization. Derived data system.

## Warnings
Be careful when working with binary log event streaming.
- Binlog stream includes changes made to all databases on the master server including sql queries with sensitive information and you may leak data from the databases. Consider deploying your database to an isolated instance.
- Transaction log represents a sequence of append-only files. It includes changes for databases/tables that you deleted and then recreated. Make sure you don't replay the phantom events in your application.
- In transactional [storage engines](https://dev.mysql.com/doc/refman/8.0/en/replication-features-transactions.html)(InnoDB, MyRocks) binlog will only include committed transactions in their commit order to guarantee consistency. Aborted or rolled-back transactions will not appear in the binary log. On the other hand in non-transactional storage engines(MyISAM) binlog will also include changes from rolled-back transactions.

## Limitations
Please note the lib currently has the following limitations:
- Supports only standard auth plugins `mysql_native_password` and `caching_sha2_password`.
- **Currently, the library doesn't fully support SSL encryption.**

## Prerequisites
Please make sure the following requirements are met:
1. The user is granted `REPLICATION SLAVE`, `REPLICATION CLIENT` privileges.
2. Binary logging is enabled(it's done by default in MySQL 8). To enable binary logging configure the following settings on the master server and restart the service:

    ```conf
    binlog_format = row
    binlog_row_image = full
    ```

   MySQL 5.6/5.7 and MariaDB 10.1 also require the following line:

    ```conf
    server-id = 1
    ```

3. Optionally in MySQL 5.6+ / MariaDB 10.5+ you can enable logging table metadata(column names, types, see `TableMetadata` class).

    ```conf
    binlog_row_metadata = full
    ```

4. Optionally you can enable logging SQL queries that precede row based events and listen to `RowsQueryEvent`.

   MySQL

    ```conf
    binlog_rows_query_log_events = on
    ```

   MariaDB

    ```conf
    binlog_annotate_row_events = on
    ```

5. Also note that there are `expire_logs_days`, `binlog_expire_logs_seconds` settings that control how long binlog files live. **By default MySQL/MariaDB have expiration time set and delete expired binlog files.** You can disable automatic purging of binlog files this way:

    ```conf
    expire_logs_days = 0
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
Alternatively, in MySQL 5.6 / MariaDB 10.5 and newer you can obtain column names by logging full metadata (see `TableMetadataEvent.Metadata`).
This way the metadata is logged with each `TableMapEvent` which impacts bandwidth. 
```conf
binlog_row_metadata = full
```

### Binlog event stream replication
Data is stored in Cells property of row events in the same order. See the C# sample project.
```csharp
var client = new BinlogClient(options =>
{
    options.Port = 3306;
    options.Username = "root";
    options.Password = "Qwertyu1";
    options.SslMode = SslMode.DISABLED;
    options.HeartbeatInterval = TimeSpan.FromSeconds(30);
    options.Blocking = true;

    // Start replication from MariaDB GTID. Recommended.
    options.Binlog = BinlogOptions.FromGtid(GtidList.Parse("0-1-270"));

    // Start replication from MySQL GTID. Recommended.
    var gtidSet = "d4c17f0c-4f11-11ea-93e3-325d3e1cd1c8:1-107, f442510a-2881-11ea-b1dd-27916133dbb2:1-7";
    options.Binlog = BinlogOptions.FromGtid(GtidSet.Parse(gtidSet));

    // Start replication from the master binlog filename and position
    options.Binlog = BinlogOptions.FromPosition("mysql-bin.000008", 195);

    // Start replication from the master last binlog filename and position.
    options.Binlog = BinlogOptions.FromEnd();

    // Start replication from the master first available(not purged) binlog filename and position.
    options.Binlog = BinlogOptions.FromStart();
});

await client.ReplicateAsync(async (binlogEvent) =>
{
    var state = client.State;

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
```
A typical transaction has the following structure.
1. `GtidEvent` if gtid mode is enabled.
2. One or many `TableMapEvent` events.
   - One or many `WriteRowsEvent` events.
   - One or many `UpdateRowsEvent` events.
   - One or many `DeleteRowsEvent` events.
3. `XidEvent` indicating commit of the transaction.

**It's best practice to use [GTID replication](https://www.percona.com/blog/2013/02/08/how-to-createrestore-a-slave-using-gtid-replication-in-mysql-5-6/) with the `FromGtid` method.** Using the approach you can correctly perform replication failover.
Note that in GTID mode `FromGtid` has the following behavior:
- `FromGtid(@@gtid_purged)` acts like `FromStart()`
- `FromGtid(@@gtid_executed)` acts like `FromEnd()`

### Reading binlog files offline
In some cases you will need to read binlog files offline from the file system.
This can be done using `BinlogReader` class.
```csharp
using (Stream stream = File.OpenRead("mariadb-bin.000002"))
{
    var reader = new BinlogReader(new MariaDbEventDeserializer(), stream);
    while (true)
    {
        var @event = await reader.ReadEventAsync();
        if (@event == null)
            break;

        await PrintEventAsync(@event);
    }
}
```

## Type mapping notes

  | MySQL Type         | .NET type            |
  | ------------------ | -------------------- |
  | BLOB types         | byte[]               |
  | GEOMETRY           | byte[]               |
  | JSON (MySQL)       | byte[], see below    |
  | JSON (MariaDB)     | byte[], see below    |
  | BIT                | bool[]               |
  | TINYINT            | byte                 |
  | SMALLINT           | short                |
  | MEDIUMINT          | int(3), see below    |
  | INT                | int                  |
  | BIGING             | long                 |
  | FLOAT              | float                |
  | DOUBLE             | double               |
  | DECIMAL            | string               |
  | VARCHAR, VARBINARY | string               |
  | CHAR               | string               |
  | ENUM               | int                  |
  | SET                | long                 |
  | YEAR               | int                  |
  | DATE               | Nullable&lt;DateTime&gt; |
  | DATETIME           | Nullable&lt;DateTime&gt; |
  | TIME               | TimeSpan             |
  | TIMESTAMP          | DateTimeOffset       |

- Invalid DATE, DATETIME values(0000-00-00) are parsed as DateTime null.
- TIME, DATETIME, TIMESTAMP (MySQL 5.6.4+) will lose microseconds when converted to .NET types as MySQL types have bigger fractional part than corresponding .NET types can store.
- Signedness of numeric columns cannot be determined in MariaDB/MySQL 5.5. The library stores all numeric columns as CLS-compliant types from the table above. The client has the information and should manually cast to `sbyte`, `ushort`, `uint` or `ulong` if necessary. The only exception is 3-byte `unsigned mediumint` which must be casted this way:

    ```csharp
    // casting unsigned mediumint columns
    uint cellValue = (uint)(int)row.Cells[0];
    uint mediumint = (cellValue << 8) >> 8;  
    ```

- JSON columns have different storage format in MariaDB and MySQL:

    ```csharp
    // MariaDB stores JSON as strings
    byte[] data = (byte[])row.Cells[0];
    string json = Encoding.UTF8.GetString(data);

    // MySQL stores JSON in binary format that needs to be parsed
    byte[] data = (byte[])row.Cells[0];
    string json = MySqlCdc.Providers.MySql.JsonParser.Parse(data);    
    ```

- GEOMETRY type is read as `byte[]` but there is no parser that constructs .NET objects.
- DECIMAL type is parsed to string as MySql decimal has bigger range(65 digits) than .NET decimal.

## Similar projects
The project is based on [mysql-binlog-connector-java](https://github.com/shyiko/mysql-binlog-connector-java) library, [MariaDB](https://mariadb.com/kb/en/replication-protocol/) and MySQL documentation.
- Java: https://github.com/shyiko/mysql-binlog-connector-java
- PHP: https://github.com/krowinski/php-mysql-replication
- Python: https://github.com/noplay/python-mysql-replication

## Supported versions
MySqlCdc supports both MariaDB & MySQL server.

  | MariaDB  | Status                   |
  | -------- |:------------------------:|
  | 10.1     | ✅ Supported             |
  | 10.2     | ✅ Supported             |
  | 10.3     | ✅ Supported             |
  | 10.4     | ✅ Supported             |
  | 10.5     | ✅ Supported             |

  | MySQL    | Status                   |
  | -------- |:------------------------:|
  | 5.6      | ✅ Supported             |
  | 5.7      | ✅ Supported             |
  | 8.0      | ✅ Supported             |

## License
The library is provided under the [MIT License](LICENSE).