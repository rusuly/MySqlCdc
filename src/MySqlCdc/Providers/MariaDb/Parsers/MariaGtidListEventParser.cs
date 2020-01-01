using System.Buffers;
using System.Collections.Generic;
using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MariaDb
{
    public class MariaGtidListEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var gtidListLength = reader.ReadLong(4);

            var gtidList = new List<string>();
            for (int i = 0; i < gtidListLength; i++)
            {
                var domainId = reader.ReadLong(4);
                var serverId = reader.ReadLong(4);
                var sequence = reader.ReadLong(8);
                var gtid = $"{domainId}-{serverId}-{sequence}";
                gtidList.Add(gtid);
            }

            return new GtidListEvent(header, gtidList);
        }
    }
}
