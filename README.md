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
- Now only the 'mysql_native_password' auth plugin is supported.

## Prerequisites
Please make sure the following requirements are met:
1. The user is granted `REPLICATION SLAVE`, `REPLICATION CLIENT` privileges.
2. The following settings must be configured on master server:
```
binlog_format=row
binlog_row_image=full
```

## Supported versions
MySql.Cdc supports both MariaDB & MySQL server.

  | MariaDB  | Status                   |
  | -------- |:------------------------:|
  | 10.3     | Did not verify           |
  | 10.4     | ✅ Supported             |


## Info
The project is based on [mysql-binlog-connector-java](https://github.com/shyiko/mysql-binlog-connector-java) library, MariaDB and MySQL  documentation.

Has a third-party dependency [Pipelines.Sockets.Unofficial](https://github.com/mgravell/Pipelines.Sockets.Unofficial) by Marc Gravell and optimized to use [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines/) as described in his [series of posts](https://blog.marcgravell.com/2018/07/pipe-dreams-part-1.html).

## License
The library is provided under the [MIT License](LICENSE).