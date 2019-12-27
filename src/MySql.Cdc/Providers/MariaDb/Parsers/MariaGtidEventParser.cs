using System.Buffers;
using MySql.Cdc.Events;
using MySql.Cdc.Parsers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Providers.MariaDb
{
    public class MariaGtidEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var sequence = reader.ReadLong(8);
            var domainId = reader.ReadLong(4);
            var gtid = $"{domainId}-{header.ServerId}-{sequence}";

            var flags = reader.ReadInt(1);

            return new GtidEvent(header, gtid, flags);
        }
    }
}
