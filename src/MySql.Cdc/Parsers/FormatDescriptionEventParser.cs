using System.Buffers;
using MySql.Cdc.Constants;
using MySql.Cdc.Events;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Parsers
{
    public class FormatDescriptionEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var binlogVersion = reader.ReadInt(2);
            var serverVersion = reader.ReadString(50).Trim((char)0);

            // Redundant timestamp & header length which is always 19
            reader.Skip(5);

            // Get size of the event payload to determine beginning of the checksum part
            reader.Skip((int)EventType.FORMAT_DESCRIPTION_EVENT - 1);
            var eventPayloadLength = reader.ReadInt(1);

            var checksumType = ChecksumType.None;
            if (eventPayloadLength != header.EventLength - EventConstants.HeaderSize)
            {
                reader.Skip(eventPayloadLength - (2 + 50 + 4 + 1 + (int)EventType.FORMAT_DESCRIPTION_EVENT));
                checksumType = (ChecksumType)reader.ReadInt(1);
            }

            return new FormatDescriptionEvent(header, binlogVersion, serverVersion, checksumType);
        }
    }
}
