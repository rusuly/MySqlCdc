using System.Buffers;
using MySql.Cdc.Events;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Parsers
{
    public class QueryEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var threadId = reader.ReadLong(4);
            var duration = reader.ReadLong(4);

            // DatabaseName length is null terminated
            reader.Skip(1);

            var errorCode = reader.ReadInt(2);
            var statusVariableLength = reader.ReadInt(2);
            var statusVariables = reader.ReadByteArraySlow(statusVariableLength);
            var databaseName = reader.ReadNullTerminatedString();
            var sqlStatement = reader.ReadStringToEndOfFile();

            return new QueryEvent(header, threadId, duration, errorCode, statusVariables, databaseName, sqlStatement);
        }
    }
}
