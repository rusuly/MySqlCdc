using System;
using System.IO;
using System.Text;
using MySql.Cdc.Constants;

namespace MySql.Cdc.Protocol
{
    /// <summary>
    /// Creates packet for a command from sequence of writes.
    /// We avoid Span<T> complexity as the protocol isn't write-intensive. 
    /// </summary>
    public class PacketWriter : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly byte _sequenceNumber;

        public PacketWriter(byte sequenceNumber)
        {
            _stream = new MemoryStream();
            _sequenceNumber = sequenceNumber;

            // Reserve space for packet header
            for (int i = 0; i < PacketConstants.HeaderSize; i++)
                _stream.WriteByte(0);
        }

        /// <summary>
        /// Writes byte to the stream.
        /// </summary>
        public void WriteByte(byte value)
        {
            _stream.WriteByte(value);
        }

        /// <summary>
        /// Writes byte array to the stream.
        /// </summary>
        public void WriteByteArray(byte[] array)
        {
#if NETSTANDARD2_1
            _stream.Write(array);
#else
            _stream.Write(array, 0, array.Length);
#endif
        }

        /// <summary>
        /// Writes int in little-endian format.
        /// </summary>
        public void WriteInt(int number, int length)
        {
            for (int i = 0; i < length; i++)
            {
                byte value = (byte)(0xFF & ((uint)number >> (i << 3)));
                _stream.WriteByte(value);
            }
        }

        /// <summary>
        /// Writes long in little-endian format.
        /// </summary>
        public void WriteLong(long number, int length)
        {
            for (int i = 0; i < length; i++)
            {
                byte value = (byte)(0xFF & ((ulong)number >> (i << 3)));
                _stream.WriteByte(value);
            }
        }

        /// <summary>
        /// Writes end-of-file length string.
        /// </summary>
        public void WriteString(string value)
        {
            if (value == null)
                return;

            WriteByteArray(Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Writes null-terminated string.
        /// </summary>
        public void WriteNullTerminatedString(string value)
        {
            WriteByteArray(Encoding.UTF8.GetBytes(value));
            _stream.WriteByte(PacketConstants.NullTerminator);
        }

        public byte[] CreatePacket()
        {
            //After body size is known we can fill packet header
            _stream.Position = 0;

            // Write header size
            WriteInt((int)_stream.Length - PacketConstants.HeaderSize, 3);

            // Write sequence number
            _stream.WriteByte(_sequenceNumber);

            return _stream.ToArray();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}
