using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Parses <see cref="IntVarEvent"/> events.
    /// Supports all versions of MariaDB and MySQL.
    /// </summary>
    public class IntVarEventParser : IEventParser
    {
        /// <summary>
        /// Parses <see cref="IntVarEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var type = reader.ReadInt(1);
            var value = reader.ReadLong(8);

            return new IntVarEvent(header, type, value);
        }
    }
}
