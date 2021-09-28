namespace MySqlCdc.Events;

/// <summary>
/// Generated when an auto increment column or LAST_INSERT_ID() function are used.
/// <a href="https://mariadb.com/kb/en/library/intvar_event/">See more</a>
/// </summary>
public class IntVarEvent : BinlogEvent
{
    /// <summary>
    /// Gets type.
    /// 0x00 - Invalid value.
    /// 0x01 - LAST_INSERT_ID.
    /// 0x02 - Insert id (auto_increment).
    /// </summary>
    public byte Type { get; }

    /// <summary>
    /// Gets value.
    /// </summary>
    public long Value { get; }

    /// <summary>
    /// Creates a new <see cref="IntVarEvent"/>.
    /// </summary>
    public IntVarEvent(EventHeader header, byte type, long value) : base(header)
    {
        Type = type;
        Value = value;
    }
}