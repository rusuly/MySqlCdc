using System.Buffers;
using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Written as the first event in binlog file or when replication is started.
    /// <see cref="https://mariadb.com/kb/en/library/format_description_event/"/>
    /// <see cref="https://mariadb.com/kb/en/library/5-slave-registration/#events-transmission-after-com_binlog_dump"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/format-description-event.html"/>
    /// </summary>
    public class FormatDescriptionEvent : BinlogEvent
    {
        public int BinlogVersion { get; private set; }
        public string ServerVersion { get; private set; }

        /// <summary>
        /// The value is redundant copy of Header.Timestamp.
        /// </summary>
        public long Timestamp { get; private set; }

        /// <summary>
        /// The value should always be 19.
        /// </summary>
        public int HeaderLength { get; private set; }
        public ChecksumType ChecksumType { get; private set; }

        public FormatDescriptionEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);

            BinlogVersion = reader.ReadInt(2);
            ServerVersion = reader.ReadString(50).Trim((char)0);
            Timestamp = reader.ReadLong(4);
            HeaderLength = reader.ReadInt(1);

            // Get size of the event payload to determine beginning of the checksum part
            reader.Skip((int)EventType.FORMAT_DESCRIPTION_EVENT - 1);
            var eventPayloadLength = reader.ReadInt(1);

            if (eventPayloadLength == Header.EventLength - HeaderLength)
            {
                ChecksumType = ChecksumType.None;
            }
            else
            {
                reader.Skip(eventPayloadLength - (2 + 50 + 4 + 1 + (int)EventType.FORMAT_DESCRIPTION_EVENT));
                ChecksumType = (ChecksumType)reader.ReadInt(1);
            }            
        }
    }
}
