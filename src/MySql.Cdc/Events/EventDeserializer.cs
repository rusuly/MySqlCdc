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

        protected readonly Dictionary<EventType, IEventParser> EventParsers
            = new Dictionary<EventType, IEventParser>();

        public EventDeserializer()
        {
            EventParsers[EventType.FORMAT_DESCRIPTION_EVENT] = new FormatDescriptionEventParser();
            EventParsers[EventType.ROTATE_EVENT] = new RotateEventParser();
            EventParsers[EventType.INTVAR_EVENT] = new IntVarEventParser();
            EventParsers[EventType.QUERY_EVENT] = new QueryEventParser();
            EventParsers[EventType.XID_EVENT] = new XidEventParser();
            EventParsers[EventType.HEARTBEAT_LOG_EVENT] = new HeartbeatEventParser();
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

            return binlogEvent;
        }
    }
}
