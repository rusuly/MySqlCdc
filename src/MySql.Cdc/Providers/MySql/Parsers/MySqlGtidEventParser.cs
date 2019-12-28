using System.Buffers;
using System.Text;
using MySql.Cdc.Events;
using MySql.Cdc.Parsers;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Providers.MySql
{
    public class MySqlGtidEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);

            var flags = reader.ReadInt(1);
            var sourceId = reader.ReadByteArraySlow(16);
            var transactionId = reader.ReadLong(8);

            var sb = new StringBuilder();
            for (int i = 0; i < sourceId.Length; i++)
            {
                if (i == 4 || i == 6 || i == 8 || i == 10)
                {
                    sb.Append("-");
                }

                sb.AppendFormat("{0:x2}", sourceId[i]);
            }
            sb.Append($":{transactionId}");

            var gtid = sb.ToString();
            return new GtidEvent(header, gtid, flags);
        }
    }
}
