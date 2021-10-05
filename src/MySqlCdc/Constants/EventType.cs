using MySqlCdc.Events;

namespace MySqlCdc.Constants;

/// <summary>
/// Binlog event types.
/// See <a href="https://mariadb.com/kb/en/library/2-binlog-event-header/">event header docs</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/binlog-event-type.html">list of event types</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/rows-event.html#write-rows-eventv2">rows event docs</a>
/// </summary>
public enum EventType
{
    /// <summary>
    /// Identifies <see cref="Events.QueryEvent"/>.
    /// </summary>
    QueryEvent = 2,

    /// <summary>
    /// Identifies StopEvent.
    /// </summary>
    StopEvent = 3,

    /// <summary>
    /// Identifies <see cref="Events.RotateEvent"/>.
    /// </summary>
    RotateEvent = 4,

    /// <summary>
    /// Identifies <see cref="IntVarEvent"/>.
    /// </summary>
    IntvarEvent = 5,

    /// <summary>
    /// Identifies RandEvent.
    /// </summary>
    RandEvent = 13,

    /// <summary>
    /// Identifies UserVarEvent.
    /// </summary>
    UserVarEvent = 14,

    /// <summary>
    /// Identifies <see cref="Events.FormatDescriptionEvent"/>.
    /// </summary>
    FormatDescriptionEvent = 15,

    /// <summary>
    /// Identifies <see cref="Events.XidEvent"/>.
    /// </summary>
    XidEvent = 16,

    /// <summary>
    /// Identifies <see cref="Events.TableMapEvent"/>.
    /// </summary>
    TableMapEvent = 19,

    /// <summary>
    /// Identifies <see cref="WriteRowsEvent"/> in MariaDB and MySQL from 5.1.15 to 5.6. 
    /// </summary>
    WriteRowsEventV1 = 23,

    /// <summary>
    /// Identifies <see cref="UpdateRowsEvent"/> in MariaDB and MySQL from 5.1.15 to 5.6. 
    /// </summary>
    UpdateRowsEventV1 = 24,

    /// <summary>
    /// Identifies <see cref="DeleteRowsEvent"/> in MariaDB and MySQL from 5.1.15 to 5.6. 
    /// </summary>
    DeleteRowsEventV1 = 25,

    /// <summary>
    /// Identifies <see cref="Events.HeartbeatEvent"/>.
    /// </summary>
    HeartbeatEvent = 27,

    /// <summary>
    /// Identifies <see cref="RowsQueryEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlRowsQueryEvent = 29,

    /// <summary>
    /// Identifies <see cref="WriteRowsEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlWriteRowsEventV2 = 30,

    /// <summary>
    /// Identifies <see cref="UpdateRowsEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlUpdateRowsEventV2 = 31,

    /// <summary>
    /// Identifies <see cref="DeleteRowsEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlDeleteRowsEventV2 = 32,

    /// <summary>
    /// Identifies <see cref="GtidEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlGtidEvent = 33,

    /// <summary>
    /// Identifies <see cref="PreviousGtidsEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlPreviousGtidsEvent = 35,

    /// <summary>
    /// Identifies <see cref="XaPrepareEvent"/> in MySQL from 5.6 to 8.0. 
    /// </summary>
    MySqlXaPrepareEvent = 38,

    /// <summary>
    /// Identifies <see cref="RowsQueryEvent"/> in MariaDB. 
    /// </summary>
    MariaDbAnnotateRowsEvent = 160,

    /// <summary>
    /// Identifies binlog checkpoint event in MariaDB. 
    /// </summary>
    MariaDbBinlogCheckpointEvent = 161,

    /// <summary>
    /// Identifies <see cref="GtidEvent"/> in MariaDB. 
    /// </summary>
    MariaDbGtidEvent = 162,

    /// <summary>
    /// Identifies <see cref="GtidListEvent"/> in MariaDB. 
    /// </summary>
    MariaDbGtidListEvent = 163,

    /// <summary>
    /// Identifies encryption start event in MariaDB. 
    /// </summary>
    MariaDbStartEncryptionEvent = 164
}