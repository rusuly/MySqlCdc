namespace MySqlCdc.Events
{
    /// <summary>
    /// Generated when an auto increment column or LAST_INSERT_ID() function are used.
    /// <see cref="https://mariadb.com/kb/en/library/intvar_event/"/>
    /// </summary>
    public class IntVarEvent : BinlogEvent
    {
        /// <summary>
        /// 0x00 - Invalid value.
        /// 0x01 - LAST_INSERT_ID.
        /// 0x02 - Insert id (auto_increment).
        /// </summary>
        public int Type { get; }
        public long Value { get; }

        public IntVarEvent(EventHeader header, int type, long value) : base(header)
        {
            Type = type;
            Value = value;
        }
    }
}
