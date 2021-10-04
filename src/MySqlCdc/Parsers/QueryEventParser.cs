using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="QueryEvent"/> events.
/// Supports all versions of MariaDB and MySQL.
/// </summary>
public class QueryEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="QueryEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        long threadId = reader.ReadUInt32LittleEndian();
        long duration = reader.ReadUInt32LittleEndian();

        // DatabaseName length
        var databaseNameLength = reader.ReadByte();

        int errorCode = reader.ReadUInt16LittleEndian();
        int statusVariableLength = reader.ReadUInt16LittleEndian();
        var statusVariables = reader.ReadByteArraySlow(statusVariableLength);
        
        // DatabaseName is null terminated
        var databaseName = reader.ReadString(databaseNameLength);
        reader.Advance(1);
        
        var sqlStatement = reader.ReadStringToEndOfFile();
        return new QueryEvent(header, threadId, duration, errorCode, statusVariables, databaseName, sqlStatement);
    }
}