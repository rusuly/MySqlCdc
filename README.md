# MySql.Cdc
MySql binlog Change Data Capture (CDC) connector for .NET Core

Acts as MySql replication client streaming binlog events in real-time.

Designed for reactive push-model applications, event sourcing or derived data systems.

## Warnings
Be carefull when working with binary log event streaming.
- Binlog stream includes changes made to all databases on the master server including sql queries with sensitive information and you may leak data from the databases. Consider deploying your database to an isolated instance.
- Binlog is an append-only file that contains changes to already deleted databases/tables/rows. Make sure you don't reproduce the logs in your application.

## Limitations
Please note the lib currently has the following limitations:
- Packet compression is not supported.
- Reading a binlog file offline is not supported.
- Automatic failover is not not supported.
- Multi-source replication & multi-master topology setup are not supported.
- Now only the `mysql_native_password` auth plugin is supported.

## Prerequisites
Please make sure the following requirements are met:
1. The user is granted `REPLICATION SLAVE`, `REPLICATION CLIENT` privileges.
2. The following settings must be configured on master server:
```
binlog_format=row
binlog_row_image=full
```

## Type mapping notes

  | MySQL Type         | .NET type            |
  | ------------------ |:--------------------:|
  | DECIMAL            | Not supported ❌     |
  | GEOMETRY           | Not supported ❌     |
  | JSON               | Not supported ❌     |
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
MySql.Cdc supports both MariaDB & MySQL server.

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