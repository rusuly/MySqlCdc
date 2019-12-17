using System.Buffers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Events
{
    /// <summary>
    /// Represents sql statement in binary log.
    /// <see cref="https://mariadb.com/kb/en/library/query_event/"/>
    /// </summary>
    public class QueryEvent : BinlogEvent
    {
        public long ThreadId { get; private set; }
        public long Duration { get; private set; }
        public int DatabaseNameLength { get; private set; }
        public int ErrorCode { get; private set; }
        public int StatusVariableLength { get; private set; }
        public byte[] StatusVariables { get; private set; }
        public string DatabaseName { get; private set; }
        public string SqlStatement { get; private set; }

        public QueryEvent(EventHeader header, ReadOnlySequence<byte> sequence) : base(header)
        {
            var reader = new PacketReader(sequence);

            ThreadId = reader.ReadLong(4);
            Duration = reader.ReadLong(4);
            DatabaseNameLength = reader.ReadInt(1);
            ErrorCode = reader.ReadInt(2);
            StatusVariableLength = reader.ReadInt(2);
            StatusVariables = reader.ReadByteArraySlow(StatusVariableLength);
            DatabaseName = reader.ReadNullTerminatedString();
            SqlStatement = reader.ReadStringToEndOfFile();
        }
    }
}
