using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Last event in a binlog file which points to next binlog file.
    /// Fake version is also returned when replication is started.
    /// <see cref="https://mariadb.com/kb/en/library/rotate_event/"/>
    /// </summary>
    public class RotateEvent : BinlogEvent
    {
        public long BinlogPosition { get; private set; }
        public string BinlogFilename { get; private set; }

        public RotateEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);

            BinlogPosition = reader.ReadLong(8);
            BinlogFilename = reader.ReadStringToEndOfFile();
        }
    }
}
