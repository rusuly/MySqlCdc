using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Events
{
    /// <summary>
    /// The event has table defition for row events.
    /// <a href="https://mariadb.com/kb/en/library/table_map_event/">See more</a>
    /// </summary>
    public class TableMapEvent : BinlogEvent
    {
        /// <summary>
        /// Gets id of the changed table
        /// </summary>
        public long TableId { get; }

        /// <summary>
        /// Gets database name of the changed table
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// Gets name of the changed table
        /// </summary>
        public string TableName { get; }

        /// <summary>
        /// Gets column types of the changed table
        /// </summary>
        public byte[] ColumnTypes { get; }

        /// <summary>
        /// Gets columns metadata
        /// </summary>
        public int[] ColumnMetadata { get; }

        /// <summary>
        /// Gets columns nullability
        /// </summary>
        public bool[] NullBitmap { get; }

        /// <summary>
        /// Gets table metadata for MySQL 5.6+
        /// </summary>
        public TableMetadata TableMetadata { get; }

        /// <summary>
        /// Creates a new <see cref="TableMapEvent"/>.
        /// </summary>
        public TableMapEvent(
            EventHeader header,
            long tableId,
            string databaseName,
            string tableName,
            byte[] columnTypes,
            int[] columnMetadata,
            bool[] nullBitmap,
            TableMetadata tableMetadata) : base(header)
        {
            TableId = tableId;
            DatabaseName = databaseName;
            TableName = tableName;
            ColumnTypes = columnTypes;
            ColumnMetadata = columnMetadata;
            NullBitmap = nullBitmap;
            TableMetadata = tableMetadata;
        }
    }
}
