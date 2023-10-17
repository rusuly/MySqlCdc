using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

/// <summary>
/// Parses <see cref="UserVarEvent"/> events.
/// Supports all versions of MariaDB and MySQL.
/// </summary>
public class UserVarEventParser : IEventParser
{
    /// <summary>
    /// Parses <see cref="UserVarEvent"/> from the buffer.
    /// </summary>
    public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
    {
        int nameLength = (int)reader.ReadUInt32LittleEndian();
        string name = reader.ReadString(nameLength);

        bool isNull = reader.ReadByte() != 0; // 0 indicates there is a value
        if (isNull)
            return new UserVarEvent(name, null);

        byte variableType = reader.ReadByte();
        int collationNumber = (int)reader.ReadUInt32LittleEndian();

        int valueLength = (int)reader.ReadUInt32LittleEndian();
        string value = reader.ReadString(valueLength);

        byte flags = reader.ReadByte();

        return new UserVarEvent(name, new VariableValue(variableType, collationNumber, value, flags));
    }
}