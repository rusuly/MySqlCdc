using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Events;

/// <summary>
/// Binlog event header version 4. Header size is 19 bytes.
/// See <a href="https://mariadb.com/kb/en/library/2-binlog-event-header/">MariaDB docs</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/binlog-version.html">MySQL docs</a>
/// </summary>
public record EventHeader(
    long Timestamp,
    EventType EventType,
    long ServerId,
    long EventLength,
    long NextEventPosition,
    int EventFlags)
{
    /// <summary>
    /// Provides creation time in seconds from Unix.
    /// </summary>
    public long Timestamp { get; } = Timestamp;

    /// <summary>
    /// Gets type of the binlog event.
    /// </summary>
    public EventType EventType { get; } = EventType;

    /// <summary>
    /// Gets id of the server that created the event.
    /// </summary>
    public long ServerId { get; } = ServerId;

    /// <summary>
    /// Gets event length (header + event + checksum).
    /// </summary>
    public long EventLength { get; } = EventLength;

    /// <summary>
    /// Gets file position of next event.
    /// </summary>
    public long NextEventPosition { get; } = NextEventPosition;

    /// <summary>
    /// Gets event flags. See <a href="https://mariadb.com/kb/en/2-binlog-event-header/#event-flag">documentation</a>.
    /// </summary>
    public int EventFlags { get; } = EventFlags;

    /// <summary>
    /// Creates a new <see cref="EventHeader"/>.
    /// </summary>
    public static EventHeader Read(ref PacketReader reader)
    {
        return new EventHeader(
            reader.ReadUInt32LittleEndian(),
            (EventType)reader.ReadByte(),
            reader.ReadUInt32LittleEndian(),
            reader.ReadUInt32LittleEndian(),
            reader.ReadUInt32LittleEndian(),
            reader.ReadUInt16LittleEndian()
        );
    }
}