using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;
using MySqlCdc.Metadata;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="TableMapEvent"/> events.
/// Supports all versions of MariaDB and MySQL 5.0+.
/// </summary>
public class TableMapEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="TableMapEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        var tableId = reader.ReadLongLittleEndian(6);

        // Reserved bytes
        reader.Advance(2);
        
        // DatabaseName is null terminated
        var databaseNameLength = reader.ReadByte();
        var databaseName = reader.ReadString(databaseNameLength);
        reader.Advance(1);

        // TableName is null terminated
        var tableNameLength = reader.ReadByte();
        var tableName = reader.ReadString(tableNameLength);
        reader.Advance(1);

        var columnsNumber = reader.ReadLengthEncodedNumber();
        var columnTypes = reader.ReadByteArraySlow(columnsNumber);

        var metadataLength = reader.ReadLengthEncodedNumber();
        var metadata = ParseMetadata(ref reader, columnTypes);

        var nullBitmap = reader.ReadBitmapLittleEndian(columnsNumber);

        TableMetadata? tableMetadata = null;
        if (!reader.IsEmpty())
        {
            // Table metadata is supported in MySQL 8.0.1+ and MariaDB 10.5+.
            tableMetadata = new TableMetadata(ref reader, columnTypes);
        }

        return new TableMapEvent(tableId, databaseName, tableName, columnTypes, metadata, nullBitmap, tableMetadata);
    }

    private int[] ParseMetadata(ref PacketReader reader, byte[] columnTypes)
    {
        int[] metadata = new int[columnTypes.Length];
        for (int i = 0; i < columnTypes.Length; i++)
        {
            // See https://mariadb.com/kb/en/library/rows_event_v1/#column-data-formats
            switch ((ColumnType)columnTypes[i])
            {
                case ColumnType.Geometry:
                case ColumnType.Json:
                case ColumnType.TinyBlob:
                case ColumnType.MediumBlob:
                case ColumnType.LongBlob:
                case ColumnType.Blob:
                case ColumnType.Float:
                case ColumnType.Double:
                case ColumnType.TimeStamp2:
                case ColumnType.DateTime2:
                case ColumnType.Time2:
                    metadata[i] = reader.ReadByte();
                    break;

                case ColumnType.Bit:
                case ColumnType.VarChar:
                case ColumnType.VarString:
                case ColumnType.NewDecimal:
                    metadata[i] = reader.ReadUInt16LittleEndian();
                    break;

                case ColumnType.Enum:
                case ColumnType.Set:
                case ColumnType.String:
                    metadata[i] = reader.ReadUInt16BigEndian();
                    break;

                default:
                    metadata[i] = 0;
                    break;
            }
        }
        return metadata;
    }
}