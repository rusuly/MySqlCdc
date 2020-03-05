using MySqlCdc.Events;

namespace MySqlCdc.Constants
{
    /// <summary>
    /// Binlog event types.
    /// See <a href="https://mariadb.com/kb/en/library/2-binlog-event-header/">event header docs</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/binlog-event-type.html">list of event types</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/rows-event.html#write-rows-eventv2">rows event docs</a>
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Identifies <see cref="QueryEvent"/>.
        /// </summary>
        QUERY_EVENT = 0x02,

        /// <summary>
        /// Identifies StopEvent.
        /// </summary>
        STOP_EVENT = 0x03,

        /// <summary>
        /// Identifies <see cref="RotateEvent"/>.
        /// </summary>
        ROTATE_EVENT = 0x04,

        /// <summary>
        /// Identifies <see cref="XidEvent"/>.
        /// </summary>
        XID_EVENT = 0x10,

        /// <summary>
        /// Identifies RandEvent.
        /// </summary>
        RAND_EVENT = 0x0d,

        /// <summary>
        /// Identifies UserVarEvent.
        /// </summary>
        USER_VAR_EVENT = 0x0e,

        /// <summary>
        /// Identifies <see cref="FormatDescriptionEvent"/>.
        /// </summary>
        FORMAT_DESCRIPTION_EVENT = 0x0f,

        /// <summary>
        /// Identifies <see cref="TableMapEvent"/>.
        /// </summary>
        TABLE_MAP_EVENT = 0x13,

        /// <summary>
        /// Identifies <see cref="HeartbeatEvent"/>.
        /// </summary>
        HEARTBEAT_EVENT = 0x1b,

        /// <summary>
        /// Identifies <see cref="IntVarEvent"/>.
        /// </summary>
        INTVAR_EVENT = 0x05,


        /// <summary>
        /// Identifies <see cref="WriteRowsEvent"/> in MariaDB and MySQL from 5.1.15 to 5.6. 
        /// </summary>
        WRITE_ROWS_EVENT_V1 = 23,

        /// <summary>
        /// Identifies <see cref="UpdateRowsEvent"/> in MariaDB and MySQL from 5.1.15 to 5.6. 
        /// </summary>
        UPDATE_ROWS_EVENT_V1 = 24,

        /// <summary>
        /// Identifies <see cref="DeleteRowsEvent"/> in MariaDB and MySQL from 5.1.15 to 5.6. 
        /// </summary>
        DELETE_ROWS_EVENT_V1 = 25,


        /// <summary>
        /// Identifies <see cref="RowsQueryEvent"/> in MySQL from 5.6 to 8.0. 
        /// </summary>
        MYSQL_ROWS_QUERY_EVENT = 29,

        /// <summary>
        /// Identifies <see cref="WriteRowsEvent"/> in MySQL from 5.6 to 8.0. 
        /// </summary>
        MYSQL_WRITE_ROWS_EVENT_V2 = 30,

        /// <summary>
        /// Identifies <see cref="UpdateRowsEvent"/> in MySQL from 5.6 to 8.0. 
        /// </summary>
        MYSQL_UPDATE_ROWS_EVENT_V2 = 31,

        /// <summary>
        /// Identifies <see cref="DeleteRowsEvent"/> in MySQL from 5.6 to 8.0. 
        /// </summary>
        MYSQL_DELETE_ROWS_EVENT_V2 = 32,

        /// <summary>
        /// Identifies <see cref="GtidEvent"/> in MySQL from 5.6 to 8.0. 
        /// </summary>
        MYSQL_GTID_EVENT = 33,

        /// <summary>
        /// Identifies <see cref="PreviousGtidsEvent"/> in MySQL from 5.6 to 8.0. 
        /// </summary>
        MYSQL_PREVIOUS_GTIDS_EVENT = 35,


        /// <summary>
        /// Identifies <see cref="RowsQueryEvent"/> in MariaDB. 
        /// </summary>
        MARIADB_ANNOTATE_ROWS_EVENT = 160,

        /// <summary>
        /// Identifies binlog checkpoint event in MariaDB. 
        /// </summary>
        MARIADB_BINLOG_CHECKPOINT_EVENT = 161,

        /// <summary>
        /// Identifies <see cref="GtidEvent"/> in MariaDB. 
        /// </summary>
        MARIADB_GTID_EVENT = 162,

        /// <summary>
        /// Identifies <see cref="GtidListEvent"/> in MariaDB. 
        /// </summary>
        MARIADB_GTID_LIST_EVENT = 163,

        /// <summary>
        /// Identifies encryption start event in MariaDB. 
        /// </summary>
        MARIADB_START_ENCRYPTION_EVENT = 164
    }
}
