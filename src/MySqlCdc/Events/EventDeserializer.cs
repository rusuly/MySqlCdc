using System;
using System.Collections.Generic;
using MySqlCdc.Checksum;
using MySqlCdc.Constants;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Base class for event deserializers.
    /// </summary>
    public abstract class EventDeserializer
    {
        /// <summary>
        /// Gets checksum algorithm type used in a binlog file.
        /// </summary>
        internal IChecksumStrategy ChecksumStrategy { get; set; } = new NoneChecksum();

        /// <summary>
        /// Gets TableMapEvent cache required in row events.
        /// </summary>
        protected readonly Dictionary<long, TableMapEvent> TableMapCache
            = new Dictionary<long, TableMapEvent>();

        /// <summary>
        /// Gets event parsers registry.
        /// </summary>
        protected readonly Dictionary<EventType, IEventParser> EventParsers
            = new Dictionary<EventType, IEventParser>();

        /// <summary>
        /// Creates a new <see cref="EventDeserializer"/>.
        /// </summary>
        protected EventDeserializer()
        {
            EventParsers[EventType.FORMAT_DESCRIPTION_EVENT] = new FormatDescriptionEventParser();
            EventParsers[EventType.TABLE_MAP_EVENT] = new TableMapEventParser();
            EventParsers[EventType.HEARTBEAT_EVENT] = new HeartbeatEventParser();
            EventParsers[EventType.ROTATE_EVENT] = new RotateEventParser();

            EventParsers[EventType.INTVAR_EVENT] = new IntVarEventParser();
            EventParsers[EventType.QUERY_EVENT] = new QueryEventParser();
            EventParsers[EventType.XID_EVENT] = new XidEventParser();

            // Rows events used in MariaDB and MySQL from 5.1.15 to 5.6.
            EventParsers[EventType.WRITE_ROWS_EVENT_V1] = new WriteRowsEventParser(TableMapCache, 1);
            EventParsers[EventType.UPDATE_ROWS_EVENT_V1] = new UpdateRowsEventParser(TableMapCache, 1);
            EventParsers[EventType.DELETE_ROWS_EVENT_V1] = new DeleteRowsEventParser(TableMapCache, 1);

            // Rows events used only in MySQL from 5.6 to 8.0.
            EventParsers[EventType.MYSQL_WRITE_ROWS_EVENT_V2] = new WriteRowsEventParser(TableMapCache, 2);
            EventParsers[EventType.MYSQL_UPDATE_ROWS_EVENT_V2] = new UpdateRowsEventParser(TableMapCache, 2);
            EventParsers[EventType.MYSQL_DELETE_ROWS_EVENT_V2] = new DeleteRowsEventParser(TableMapCache, 2);
        }

        /// <summary>
        /// Constructs a <see cref="IBinlogEvent"/> from packet buffer.
        /// </summary>
        public virtual IBinlogEvent DeserializeEvent(ref PacketReader reader)
        {
            var eventHeader = new EventHeader(ref reader);

            // Consider verifying checksum
            // ChecksumType.Verify(eventBuffer, checksumBuffer);
            reader.SliceFromEnd(0, ChecksumStrategy.Length);

            IBinlogEvent binlogEvent = null;
            if (EventParsers.TryGetValue(eventHeader.EventType, out var eventParser))
            {
                binlogEvent = eventParser.ParseEvent(eventHeader, ref reader);
            }
            else
            {
                binlogEvent = new UnknownEvent(eventHeader);
            }

            if (binlogEvent is FormatDescriptionEvent formatEvent)
            {
                ChecksumStrategy = formatEvent.ChecksumType switch
                {
                    ChecksumType.NONE => new NoneChecksum(),
                    ChecksumType.CRC32 => new Crc32Checksum(),
                    _ => throw new InvalidOperationException("The master checksum type is not supported.")
                };
            }
            if (binlogEvent is TableMapEvent tableMapEvent)
            {
                TableMapCache[tableMapEvent.TableId] = tableMapEvent;
            }

            return binlogEvent;
        }
    }
}
