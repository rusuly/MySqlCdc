using System.Collections;

namespace MySqlCdc.Events
{
    /// <summary>
    /// The event has table defition for row events.
    /// <see cref="https://mariadb.com/kb/en/library/table_map_event/"/>
    /// </summary>
    public class TableMapEvent : BinlogEvent
    {
        public long TableId { get; }
        public string DatabaseName { get; }
        public string TableName { get; }
        public byte[] ColumnTypes { get; }
        public int[] ColumnMetadata { get; }
        public BitArray NullBitmap { get; }

        public TableMapEvent(
            EventHeader header,
            long tableId,
            string databaseName,
            string tableName,
            byte[] columnTypes,
            int[] columnMetadata,
            BitArray nullBitmap) : base(header)
        {
            TableId = tableId;
            DatabaseName = databaseName;
            TableName = tableName;
            ColumnTypes = columnTypes;
            ColumnMetadata = columnMetadata;
            NullBitmap = nullBitmap;
        }
    }
}
