using System.Buffers;
using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Binlog event header version 4. Header size is 19 bytes.
    /// See <a href="https://mariadb.com/kb/en/library/2-binlog-event-header/">MariaDB docs</a>
    /// See <a href="https://dev.mysql.com/doc/internals/en/binlog-version.html">MySQL docs</a>
    /// </summary>
    public class EventHeader
    {
        /// <summary>
        /// Provides creation time in seconds from Unix.
        /// </summary>
        public long Timestamp { get; }
        public EventType EventType { get; }
        public long ServerId { get; }
        public long EventLength { get; }
        public long NextEventPosition { get; }
        public int EventFlags { get; }

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
