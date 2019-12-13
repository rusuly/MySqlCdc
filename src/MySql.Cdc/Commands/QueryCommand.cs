using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Commands
{
    /// <summary>
    /// COM_QUERY sends the server an SQL statement to be executed immediately.
    /// <see cref="https://mariadb.com/kb/en/library/com_query/"/>
    /// </summary>
    public class QueryCommand : ICommand
    {
        public string Sql { get; private set; }

        public QueryCommand(string sql)
        {
            Sql = sql;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);
            writer.WriteByte((byte)CommandType.QUERY);
            writer.WriteString(Sql);
            return writer.CreatePacket();
        }
    }
}
