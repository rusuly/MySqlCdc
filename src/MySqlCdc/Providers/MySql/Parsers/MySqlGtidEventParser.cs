using System.Text;
using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MySql
{
    public class MySqlGtidEventParser : IEventParser
    {
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
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
