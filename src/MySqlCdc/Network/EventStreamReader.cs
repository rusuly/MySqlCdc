using System;
using System.Buffers;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Packets;
using MySqlCdc.Protocol;

namespace MySqlCdc.Network
{
    /// <summary>
    /// Reads binlog event packets from network stream.
    /// <a href="https://mariadb.com/kb/en/3-binlog-network-stream/">See more</a>
    /// </summary>
    internal class EventStreamReader : IEventStreamReader
    {
        private readonly EventDeserializer _eventDeserializer;

        public EventStreamReader(EventDeserializer eventDeserializer)
        {
            _eventDeserializer = eventDeserializer;
        }

        public IPacket ReadPacket(ReadOnlySequence<byte> buffer)
        {
            var packetReader = new PacketReader(buffer);
            var status = packetReader.ReadInt(1);
            buffer = buffer.Slice(1);

            try
            {
                // Network stream has 3 possible status types.
                return (ResponseType)status switch
                {
                    ResponseType.Error => new ErrorPacket(buffer),
                    ResponseType.EndOfFile => new EndOfFilePacket(buffer),
                    _ => _eventDeserializer.DeserializeEvent(buffer)
                };
            }
            catch (Exception e)
            {
                // We stop replication if deserialization throws an exception 
                // Since a derived database may end up in an inconsistent state.
                return new ExceptionPacket(e);
            }
        }
    }
}
