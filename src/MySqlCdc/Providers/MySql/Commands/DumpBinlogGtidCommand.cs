using System;
using System.Linq;
using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Commands
{
    /// <summary>
    /// Requests binlog event stream by Gtid.
    /// <a href="https://mariadb.com/kb/en/library/com_binlog_dump/">See more</a>
    /// </summary>
    internal class DumpBinlogGtidCommand : ICommand
    {
        public long ServerId { get; private set; }
        public string BinlogFilename { get; private set; }
        public long BinlogPosition { get; private set; }
        public string Gtid { get; private set; }
        public int Flags { get; private set; }

        public DumpBinlogGtidCommand(long serverId, string binlogFilename, long binlogPosition, string gtid, int flags = 0)
        {
            ServerId = serverId;
            BinlogFilename = binlogFilename;
            BinlogPosition = binlogPosition;
            Gtid = gtid;
            Flags = flags;
        }

        public byte[] CreatePacket(byte sequenceNumber)
        {
            var writer = new PacketWriter(sequenceNumber);

            writer.WriteByte((byte)CommandType.BINLOG_DUMP_GTID);
            writer.WriteInt(Flags, 2);
            writer.WriteLong(ServerId, 4);

            writer.WriteInt(BinlogFilename.Length, 4);
            writer.WriteString(BinlogFilename);
            writer.WriteLong(BinlogPosition, 8);

            var gtidSet = Gtid.Split(':');
            var interval = gtidSet[1].Split('-');

            var sourceId = StringToByteArray(gtidSet[0].Replace("-", ""));
            var start = int.Parse(interval[0]);
            var end = int.Parse(interval[1]);

            //See: https://dev.mysql.com/doc/internals/en/com-binlog-dump-gtid.html
            writer.WriteInt(8 + 16 + 8 + 16, 4);
            writer.WriteLong(1, 8);
            writer.WriteByteArray(sourceId);
            writer.WriteLong(1, 8);
            writer.WriteLong(start, 8);
            writer.WriteLong(end + 1, 8);

            return writer.CreatePacket();
        }

        private byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
}
