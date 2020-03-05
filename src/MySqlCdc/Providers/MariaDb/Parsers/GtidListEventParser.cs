using System.Collections.Generic;
using MySqlCdc.Events;
using MySqlCdc.Parsers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MariaDb
{
    /// <summary>
    /// Parses <see cref="GtidListEvent"/> events in MariaDB.
    /// </summary>
    public class GtidListEventParser : IEventParser
    {
        /// <summary>
        /// Parses <see cref="GtidListEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            long gtidListLength = (uint)reader.ReadInt32LittleEndian();

            var gtidList = new GtidList();
            for (int i = 0; i < gtidListLength; i++)
            {
                long domainId = (uint)reader.ReadInt32LittleEndian();
                long serverId = (uint)reader.ReadInt32LittleEndian();
                long sequence = reader.ReadInt64LittleEndian();

                gtidList.Gtids.Add(new Gtid(domainId, serverId, sequence));
            }

            return new GtidListEvent(header, gtidList);
        }
    }
}
