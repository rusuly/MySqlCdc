using System;
using System.Collections.Generic;
using MySqlCdc.Checksum;
using MySqlCdc.Constants;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Events;

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
    protected readonly Dictionary<long, TableMapEvent> TableMapCache = new();

    /// <summary>
    /// Gets event parsers registry.
    /// </summary>
    protected readonly Dictionary<EventType, IEventParser> EventParsers = new();

    /// <summary>
    /// Creates a new <see cref="EventDeserializer"/>.
    /// </summary>
    protected EventDeserializer()
    {
        EventParsers[EventType.FormatDescriptionEvent] = new FormatDescriptionEventParser();
        EventParsers[EventType.TableMapEvent] = new TableMapEventParser();
        EventParsers[EventType.HeartbeatEvent] = new HeartbeatEventParser();
        EventParsers[EventType.RotateEvent] = new RotateEventParser();

        EventParsers[EventType.IntvarEvent] = new IntVarEventParser();
        EventParsers[EventType.QueryEvent] = new QueryEventParser();
        EventParsers[EventType.XidEvent] = new XidEventParser();

        // Rows events used in MariaDB and MySQL from 5.1.15 to 5.6.
        EventParsers[EventType.WriteRowsEventV1] = new WriteRowsEventParser(TableMapCache, 1);
        EventParsers[EventType.UpdateRowsEventV1] = new UpdateRowsEventParser(TableMapCache, 1);
        EventParsers[EventType.DeleteRowsEventV1] = new DeleteRowsEventParser(TableMapCache, 1);

        // Rows events used only in MySQL from 5.6 to 8.0.
        EventParsers[EventType.MySqlWriteRowsEventV2] = new WriteRowsEventParser(TableMapCache, 2);
        EventParsers[EventType.MySqlUpdateRowsEventV2] = new UpdateRowsEventParser(TableMapCache, 2);
        EventParsers[EventType.MySqlDeleteRowsEventV2] = new DeleteRowsEventParser(TableMapCache, 2);
    }

    /// <summary>
    /// Constructs a <see cref="IBinlogEvent"/> from packet buffer.
    /// </summary>
    public virtual IBinlogEvent DeserializeEvent(ref PacketReader reader)
    {
        var eventHeader = new EventHeader(ref reader);

        // Consider verifying checksum
        // ChecksumType.Verify(eventBuffer, checksumBuffer);
        reader.SliceFromEnd(ChecksumStrategy.Length);

        IBinlogEvent binlogEvent = EventParsers.TryGetValue(eventHeader.EventType, out var eventParser) 
            ? eventParser.ParseEvent(eventHeader, ref reader) 
            : new UnknownEvent(eventHeader);

        if (binlogEvent is FormatDescriptionEvent formatEvent)
        {
            ChecksumStrategy = formatEvent.ChecksumType switch
            {
                ChecksumType.None => new NoneChecksum(),
                ChecksumType.Crc32 => new Crc32Checksum(),
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