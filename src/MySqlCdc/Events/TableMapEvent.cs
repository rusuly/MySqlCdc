using MySqlCdc.Metadata;

namespace MySqlCdc.Events;

/// <summary>
/// The event has table defition for row events.
/// <a href="https://mariadb.com/kb/en/library/table_map_event/">See more</a>
/// </summary>
/// <remarks>
/// Creates a new <see cref="TableMapEvent"/>.
/// </remarks>
public record TableMapEvent(
    long TableId,
    string DatabaseName,
    string TableName,
    byte[] ColumnTypes,
    int[] ColumnMetadata,
    bool[] NullBitmap,
    TableMetadata? TableMetadata) : IBinlogEvent
{
    /// <summary>
    /// Gets id of the changed table
    /// </summary>
    public long TableId { get; } = TableId;

    /// <summary>
    /// Gets database name of the changed table
    /// </summary>
    public string DatabaseName { get; } = DatabaseName;

    /// <summary>
    /// Gets name of the changed table
    /// </summary>
    public string TableName { get; } = TableName;

    /// <summary>
    /// Gets column types of the changed table
    /// </summary>
    public byte[] ColumnTypes { get; } = ColumnTypes;

    /// <summary>
    /// Gets columns metadata
    /// </summary>
    public int[] ColumnMetadata { get; } = ColumnMetadata;

    /// <summary>
    /// Gets columns nullability
    /// </summary>
    public bool[] NullBitmap { get; } = NullBitmap;

    /// <summary>
    /// Gets table metadata for MySQL 8.0.1+
    /// </summary>
    public TableMetadata? TableMetadata { get; } = TableMetadata;
}