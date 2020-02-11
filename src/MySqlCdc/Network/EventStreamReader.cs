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
            using var memoryOwner = new MemoryOwner(buffer);
            var reader = new PacketReader(memoryOwner.Memory.Span);
            var status = reader.ReadByte();

            try
            {
                // Network stream has 3 possible status types.
                return (ResponseType)status switch
                {
                    ResponseType.Ok => _eventDeserializer.DeserializeEvent(ref reader),
                    ResponseType.EndOfFile => new EndOfFilePacket(buffer.Slice(1)),
                    _ => new ErrorPacket(buffer.Slice(1)),
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
