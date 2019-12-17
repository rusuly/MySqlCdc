using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
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
        public int Type { get; private set; }
        public long Value { get; private set; }

        public IntVarEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);

            Type = reader.ReadInt(1);
            Value = reader.ReadLong(8);
        }
    }
}
