using MySql.Cdc.Constants;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Commands
{
    /// <summary>
    /// Requests binlog event stream.
    /// <see cref="https://mariadb.com/kb/en/library/com_binlog_dump/"/>
    /// </summary>
    public class DumpBinlogCommand : ICommand
    {
        public long ServerId { get; private set; }
        public string BinlogFilename { get; private set; }
        public long BinlogPosition { get; private set; }
        public int Flags { get; private set; }

        public DumpBinlogCommand(long serverId, string binlogFilename, long binlogPosition, int flags = 0)
        {
            ServerId = serverId;
            BinlogFilename = binlogFilename;
            BinlogPosition = binlogPosition;
            Flags = flags;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);
            writer.WriteByte((byte)CommandType.BINLOG_DUMP);
            writer.WriteLong(BinlogPosition, 4);
            writer.WriteInt(Flags, 2);
            writer.WriteLong(ServerId, 4);
            writer.WriteString(BinlogFilename);
            return writer.CreatePacket();
        }
    }
}
