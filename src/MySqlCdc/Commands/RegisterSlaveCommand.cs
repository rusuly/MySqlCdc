using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Commands
{
    /// <summary>
    /// Used for MariaDB Gtid replication.
    /// <see cref="https://mariadb.com/kb/en/com_register_slave/"/>
    /// <see cref="https://dev.mysql.com/doc/internals/en/com-register-slave.html"/>
    /// </summary>
    public class RegisterSlaveCommand : ICommand
    {
        public long ServerId { get; private set; }

        public RegisterSlaveCommand(long serverId)
        {
            ServerId = serverId;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);

            writer.WriteByte((byte)CommandType.REGISTER_SLAVE);
            writer.WriteLong(ServerId, 4);

            //Empty host, user, password, port, rank, masterid
            writer.WriteInt(0, 1);
            writer.WriteInt(0, 1);
            writer.WriteInt(0, 1);
            writer.WriteInt(0, 2);
            writer.WriteInt(0, 4);
            writer.WriteInt(0, 4);

            return writer.CreatePacket();
        }
    }
}
