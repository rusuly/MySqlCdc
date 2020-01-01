namespace MySqlCdc.Constants
{
    /// <summary>
    /// Binlog event types.
    /// <see cref="https://mariadb.com/kb/en/library/2-binlog-event-header/"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/binlog-event-type.html"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/rows-event.html#write-rows-eventv2"/>
    /// </summary>
    public enum EventType
    {
        QUERY_EVENT = 0x02,
        STOP_EVENT = 0x03,
        ROTATE_EVENT = 0x04,
        XID_EVENT = 0x10,
        RAND_EVENT = 0x0d,
        USER_VAR_EVENT = 0x0e,
        FORMAT_DESCRIPTION_EVENT = 0x0f,
        TABLE_MAP_EVENT = 0x13,
        HEARTBEAT_EVENT = 0x1b,
        INTVAR_EVENT = 0x05,

        // Used in MariaDB and MySQL from 5.1.15 to 5.6.x
        WRITE_ROWS_EVENT_V1 = 23,
        UPDATE_ROWS_EVENT_V1 = 24,
        DELETE_ROWS_EVENT_V1 = 25,

        // Used in MySQL only from MySQL 5.6.x
        // Extra-data fields are added compared to V1.
        MYSQL_WRITE_ROWS_EVENT_V2 = 30,
        MYSQL_UPDATE_ROWS_EVENT_V2 = 31,
        MYSQL_DELETE_ROWS_EVENT_V2 = 32,
        MYSQL_GTID_EVENT = 33,

        // Used in MariaDB only.
        MARIADB_ANNOTATE_ROWS_EVENT = 160,
        MARIADB_BINLOG_CHECKPOINT_EVENT = 161,
        MARIADB_GTID_EVENT = 162,
        MARIADB_GTID_LIST_EVENT = 163,
        MARIADB_START_ENCRYPTION_EVENT = 164
    }
}
