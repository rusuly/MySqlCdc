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

        /// <summary>
        /// Gets type of the binlog event.
        /// </summary>
        public EventType EventType { get; }

        /// <summary>
        /// Gets id of the server that created the event.
        /// </summary>
        public long ServerId { get; }

        /// <summary>
        /// Gets event length (header + event + checksum).
        /// </summary>
        public long EventLength { get; }

        /// <summary>
        /// Gets file position of next event.
        /// </summary>
        public long NextEventPosition { get; }

        /// <summary>
        /// Gets event flags. See <a href="https://mariadb.com/kb/en/2-binlog-event-header/#event-flag">documentation</a>.
        /// </summary>
        public int EventFlags { get; }

        /// <summary>
        /// Creates a new <see cref="EventHeader"/>.
        /// </summary>
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
