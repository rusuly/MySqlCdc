namespace MySqlCdc.Events;

/// <summary>
/// Represents a binlog event.
/// </summary>
public interface IBinlogEvent
{
}

/// <summary>
/// Common interface for all events from: <a href="https://mariadb.com/kb/en/rows_event_v1v2-rows_compressed_event_v1/" />
/// </summary>
public interface IBinlogRowsEvent : IBinlogEvent
{
}