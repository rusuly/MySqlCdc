using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Commands
{
    /// <summary>
    /// COM_PING sends a packet containing one byte to check that the connection is active.
    /// <see cref="https://mariadb.com/kb/en/library/com_ping/"/>
    /// </summary>
    public class PingCommand : ICommand
    {
        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);
            writer.WriteByte((byte)CommandType.PING);
            return writer.CreatePacket();
        }
    }
}
