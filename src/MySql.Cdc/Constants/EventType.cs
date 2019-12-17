namespace MySql.Cdc.Constants
{
    /// <summary>
    /// Binlog event types.
    /// <see cref="https://mariadb.com/kb/en/library/2-binlog-event-header/"/>
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
        HEARTBEAT_LOG_EVENT = 0x1b,
        INTVAR_EVENT = 0x05
    }
}
