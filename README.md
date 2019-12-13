# MySql.Cdc
MySql binlog Change Data Capture (CDC) connector for .NET Core

Acts as MySql replication client streaming binlog events in real-time.

Designed for reactive push-model applications, event sourcing or derived data systems.

## Limitations
Please note the lib currently has the following limitations:
1. Packet compression is not supported.
2. Reading a binlog file offline is not supported.
3. Automatic failover is not not supported.

## Prerequisites
Please make sure the following requirements are met:
1. The user is granted REPLICATION SLAVE, REPLICATION CLIENT privileges.
2. binlog_format = row.

## Supported versions

## Info
The project is based on [mysql-binlog-connector-java](https://github.com/shyiko/mysql-binlog-connector-java) library, MariaDB and MySQL  documentation.

Has a third-party dependency [Pipelines.Sockets.Unofficial](https://github.com/mgravell/Pipelines.Sockets.Unofficial) by Marc Gravell and optimized to use [System.IO.Pipelines](https://www.nuget.org/packages/System.IO.Pipelines/) as described in his [series of posts](https://blog.marcgravell.com/2018/07/pipe-dreams-part-1.html).