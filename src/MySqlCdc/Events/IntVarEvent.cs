namespace MySqlCdc.Events;

/// <summary>
/// Generated when an auto increment column or LAST_INSERT_ID() function are used.
/// <a href="https://mariadb.com/kb/en/library/intvar_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="IntVarEvent"/>.
/// </remarks>
public record IntVarEvent(byte Type, long Value) : IBinlogEvent
{
    /// <summary>
    /// Gets type.
    /// 0x00 - Invalid value.
    /// 0x01 - LAST_INSERT_ID.
    /// 0x02 - Insert id (auto_increment).
    /// </summary>
    public byte Type { get; } = Type;

    /// <summary>
    /// Gets value.
    /// </summary>
    public long Value { get; } = Value;
}