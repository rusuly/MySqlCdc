using MySqlCdc.Protocol;

namespace MySqlCdc.Commands;

/// <summary>
/// COM_QUERY sends the server an SQL statement to be executed immediately.
/// <a href="https://mariadb.com/kb/en/library/com_query/">See more</a>
/// </summary>
internal class QueryCommand : ICommand
{
    public string Sql { get; }

    public QueryCommand(string sql)
    {
        Sql = sql;
    }

    public byte[] Serialize()
    {
        var writer = new PacketWriter();
        writer.WriteByte((byte)CommandType.Query);
        writer.WriteString(Sql);
        return writer.CreatePacket();
    }
}