using MySqlCdc.Protocol;

namespace MySqlCdc.Commands;

/// <summary>
/// Used for MariaDB Gtid replication.
/// See <a href="https://mariadb.com/kb/en/com_register_slave/">MariaDB docs</a>
/// See <a href="https://dev.mysql.com/doc/internals/en/com-register-slave.html">MySQL docs</a>
/// </summary>
internal class RegisterSlaveCommand : ICommand
{
    public long ServerId { get; }

    public RegisterSlaveCommand(long serverId)
    {
        ServerId = serverId;
    }

    public byte[] Serialize()
    {
        var writer = new PacketWriter();

        writer.WriteByte((byte)CommandType.REGISTER_SLAVE);
        writer.WriteLongLittleEndian(ServerId, 4);

        //Empty host, user, password, port, rank, masterid
        writer.WriteIntLittleEndian(0, 1);
        writer.WriteIntLittleEndian(0, 1);
        writer.WriteIntLittleEndian(0, 1);
        writer.WriteIntLittleEndian(0, 2);
        writer.WriteIntLittleEndian(0, 4);
        writer.WriteIntLittleEndian(0, 4);

        return writer.CreatePacket();
    }
}