using System.Buffers;
using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Binlog event header version 4. Header size is 19 bytes.
    /// <see cref="https://mariadb.com/kb/en/library/2-binlog-event-header/"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/binlog-version.html"/>
    /// </summary>
    public class EventHeader
    {
        /// <summary>
        /// Provides creation time in seconds from Unix.
        /// </summary>
        public long Timestamp { get; private set; }
        public EventType EventType { get; private set; }
        public long ServerId { get; private set; }
        public long EventLength { get; private set; }
        public long NextEventPosition { get; private set; }
        public int EventFlags { get; private set; }

        public EventHeader(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);

            Timestamp = reader.ReadLong(4);
            EventType = (EventType)reader.ReadInt(1);
            ServerId = reader.ReadLong(4);
            EventLength = reader.ReadLong(4);
            NextEventPosition = reader.ReadLong(4);
            EventFlags = reader.ReadInt(2);
        }
    }
}
