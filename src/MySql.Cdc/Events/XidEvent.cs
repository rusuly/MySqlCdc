using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Generated for a commit of a transaction.
    /// <see cref="https://mariadb.com/kb/en/library/xid_event/"/>
    /// </summary>
    public class XidEvent : BinlogEvent
    {
        public long Xid { get; private set; }

        public XidEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);

            Xid = reader.ReadLong(8);
        }
    }
}
