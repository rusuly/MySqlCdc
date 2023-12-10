using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers;

    /// <summary>
    /// Parses <see cref="RandEvent"/> events.
    /// Supports all versions of MariaDB and MySQL.
    /// </summary>

    public class RandEventParser : IEventParser
    {
        /// <summary>
        /// Parses <see cref="RandEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader) 
        {
            var seed1 = reader.ReadInt64LittleEndian();
            var seed2 = reader.ReadInt64LittleEndian();

            return new RandEvent(seed1, seed2);
        }
    }
