using System.Buffers;
using MySql.Cdc.Constants;

namespace MySql.Cdc.Events
{
    public class EventDeserializer
    {
        public IBinlogEvent DeserializeEvent(ReadOnlySequence<byte> buffer)
        {
            var eventHeader = new EventHeader(buffer);
            buffer = buffer.Slice(EventConstants.HeaderSize);

            IBinlogEvent binlogEvent = null;
            if (eventHeader.EventType == EventType.ROTATE_EVENT)
            {
                binlogEvent = new RotateEvent(eventHeader, buffer);
            }
            else if (eventHeader.EventType == EventType.FORMAT_DESCRIPTION_EVENT)
            {
                binlogEvent = new FormatDescriptionEvent(eventHeader, buffer);
            }
            else if (eventHeader.EventType == EventType.QUERY_EVENT)
            {
                binlogEvent = new QueryEvent(eventHeader, buffer);
            }
            else if (eventHeader.EventType == EventType.HEARTBEAT_LOG_EVENT)
            {
                binlogEvent = new HeartbeatEvent(eventHeader, buffer);
            }
            else if (eventHeader.EventType == EventType.INTVAR_EVENT)
            {
                binlogEvent = new IntVarEvent(eventHeader, buffer);
            }
            else if (eventHeader.EventType == EventType.XID_EVENT)
            {
                binlogEvent = new XidEvent(eventHeader, buffer);
            }
            else
            {
                binlogEvent = new UnknownEvent(eventHeader, buffer);
            }
            return binlogEvent;
        }
    }
}
