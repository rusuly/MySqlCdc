using System;
using System.Buffers;
using System.Collections.Generic;
using MySql.Cdc.Checksum;
using MySql.Cdc.Constants;
using MySql.Cdc.Parsers;

namespace MySql.Cdc.Events
{
    public class EventDeserializer
    {
        /// <summary>
        /// The checksum algorithm type used in a binlog file.
        /// </summary>
        public IChecksumStrategy ChecksumStrategy { get; set; }

        /// <summary>
        /// Rows events depend on TableMapEvent that comes before them.
        /// </summary>
        protected readonly Dictionary<long, TableMapEvent> TableMapCache
            = new Dictionary<long, TableMapEvent>();

        protected readonly Dictionary<EventType, IEventParser> EventParsers
            = new Dictionary<EventType, IEventParser>();

        public EventDeserializer()
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

        public virtual IBinlogEvent DeserializeEvent(ReadOnlySequence<byte> buffer)
        {
            var eventHeader = new EventHeader(buffer.Slice(0, EventConstants.HeaderSize));
            var eventLength = eventHeader.EventLength - EventConstants.HeaderSize - ChecksumStrategy.Length;

            var eventBuffer = buffer.Slice(EventConstants.HeaderSize, eventLength);
            var checksumBuffer = buffer.Slice(eventHeader.EventLength - ChecksumStrategy.Length, ChecksumStrategy.Length);

            // Consider verifying checksum
            // ChecksumType.Verify(eventBuffer, checksumBuffer);

            IBinlogEvent binlogEvent = null;
            if (EventParsers.TryGetValue(eventHeader.EventType, out var eventParser))
            {
                binlogEvent = eventParser.ParseEvent(eventHeader, eventBuffer);
            }
            else
            {
                binlogEvent = new UnknownEvent(eventHeader);
            }

            if (binlogEvent is FormatDescriptionEvent formatEvent)
            {
                ChecksumStrategy = formatEvent.ChecksumType switch
                {
                    ChecksumType.None => new NoneChecksum(),
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
