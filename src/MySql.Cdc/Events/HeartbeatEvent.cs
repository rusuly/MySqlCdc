using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// The event is sent from master to the client for keep alive feature.
    /// <see cref="https://mariadb.com/kb/en/library/heartbeat_log_event/"/>
    /// </summary>
    public class HeartbeatEvent : BinlogEvent
    {
        public string BinlogFilename { get; private set; }

        public HeartbeatEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);

            BinlogFilename = reader.ReadStringToEndOfFile();
        }
    }
}
