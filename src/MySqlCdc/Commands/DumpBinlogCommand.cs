using MySqlCdc.Protocol;

namespace MySqlCdc.Commands;

/// <summary>
/// Requests binlog event stream.
/// <a href="https://mariadb.com/kb/en/library/com_binlog_dump/">See more</a>
/// </summary>
internal class DumpBinlogCommand : ICommand
{
    public long ServerId { get; }
    public string BinlogFilename { get; }
    public long BinlogPosition { get; }
    public int Flags { get; }

    public DumpBinlogCommand(long serverId, string binlogFilename, long binlogPosition, int flags = 0)
    {
        ServerId = serverId;
        BinlogFilename = binlogFilename;
        BinlogPosition = binlogPosition;
        Flags = flags;
    }

    public byte[] Serialize()
    {
        var writer = new PacketWriter();
        writer.WriteByte((byte)CommandType.BINLOG_DUMP);
        writer.WriteLongLittleEndian(BinlogPosition, 4);
        writer.WriteIntLittleEndian(Flags, 2);
        writer.WriteLongLittleEndian(ServerId, 4);
        writer.WriteString(BinlogFilename);
        return writer.CreatePacket();
    }
}