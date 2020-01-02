using System;
using System.Buffers;
using System.Collections;
using System.Linq;
using System.Text;
using MySqlCdc.Constants;

namespace MySqlCdc.Protocol
{
    /// <summary>
    /// Constructs server reply from byte packet response.
    /// </summary>
    public class PacketReader
    {
        private ReadOnlySequence<byte> _sequence;

        public PacketReader(ReadOnlySequence<byte> sequence)
        {
            _sequence = sequence;
        }

        /// <summary>
        /// Reads int number written in little-endian format.
        /// </summary>
        public int ReadInt(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                byte value = _sequence.Slice(i, 1).First.Span[0];
                result |= value << (i << 3);
            }
            _sequence = _sequence.Slice(length);
            return result;
        }

        /// <summary>
        /// Reads long number written in little-endian format.
        /// </summary>
        public long ReadLong(int length)
        {
            long result = 0;
            for (int i = 0; i < length; i++)
            {
                byte value = _sequence.Slice(i, 1).First.Span[0];
                result |= (long)value << (i << 3);
            }
            _sequence = _sequence.Slice(length);
            return result;
        }

        public int ReadBigEndianInt(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                byte value = _sequence.Slice(i, 1).First.Span[0];
                result = (result << 8) | (int)value;
            }
            _sequence = _sequence.Slice(length);
            return result;
        }

        public long ReadBigEndianLong(int length)
        {
            long result = 0;
            for (int i = 0; i < length; i++)
            {
                byte value = _sequence.Slice(i, 1).First.Span[0];
                result = (result << 8) | (long)value;
            }
            _sequence = _sequence.Slice(length);
            return result;
        }

        /// <summary>
        /// if (first byte < 0xFB) - Integer value is this 1 byte integer
        /// 0xFB - NULL value
        /// 0xFC - Integer value is encoded in the next 2 bytes (3 bytes total)
        /// 0xFD - Integer value is encoded in the next 3 bytes (4 bytes total)
        /// 0xFE - Integer value is encoded in the next 8 bytes (9 bytes total)
        /// </summary>
        public int? ReadLengthEncodedNumber()
        {
            int firstByte = ReadInt(1);

            if (firstByte < 0xFB)
                return firstByte;
            else if (firstByte == 0xFB)
                throw new FormatException("Length encoded integer cannot be NULL.");
            else if (firstByte == 0xFC)
                return ReadInt(2);
            else if (firstByte == 0xFD)
                return ReadInt(3);
            else if (firstByte == 0xFE)
            {
                try
                {
                    // Max theoretical length of .NET string is Int32.MaxValue
                    return checked((int)ReadLong(8));
                }
                catch (OverflowException)
                {
                    throw new FormatException("Length encoded integer cannot exceed Int32.MaxValue.");
                }
            }
            throw new FormatException($"Unexpected length-encoded integer: {firstByte}");
        }

        /// <summary>
        /// Reads fixed length string.
        /// </summary>
        public string ReadString(int length)
        {
            var str = ParseString(_sequence.Slice(0, length));
            _sequence = _sequence.Slice(length);
            return str;
        }

        /// <summary>
        /// Reads string to end of the sequence.
        /// </summary>
        public string ReadStringToEndOfFile()
        {
            var str = ParseString(_sequence);
            _sequence = _sequence.Slice(_sequence.End);
            return str;
        }

        /// <summary>
        /// Reads string terminated by 0 byte.
        /// </summary>
        public string ReadNullTerminatedString()
        {
            var position = _sequence.PositionOf(PacketConstants.NullTerminator);
            var str = ParseString(_sequence.Slice(0, position.Value));
            _sequence = _sequence.Slice(_sequence.GetPosition(1, position.Value));
            return str;
        }

        /// <summary>
        /// Reads length-encoded string.
        /// </summary>
        public string ReadLengthEncodedString()
        {
            var length = ReadLengthEncodedNumber();
            return ReadString(length.Value);
        }

        /// <summary>
        /// Reads byte array from the sequence.
        /// Allocates managed memory for the array.
        /// </summary>
        public byte[] ReadByteArraySlow(int length)
        {
            var bytes = _sequence.Slice(0, length).ToArray();
            _sequence = _sequence.Slice(length);
            return bytes;
        }

        /// <summary>
        /// To store N bits (N + 7) / 8 bytes are required
        /// </summary>
        public BitArray ReadBitmap(int bitsNumber)
        {
            var bitmapBytes = ReadByteArraySlow((bitsNumber + 7) / 8);
            return new BitArray(bitmapBytes);
        }

        public BitArray ReadBitmapBigEndian(int bitsNumber)
        {
            var bitmapBytes = ReadByteArraySlow((bitsNumber + 7) / 8);
            bitmapBytes = bitmapBytes.Reverse().ToArray();
            return new BitArray(bitmapBytes);
        }

        public bool IsEmpty()
        {
            return _sequence.IsEmpty;
        }

        public void Skip(int offset)
        {
            _sequence = _sequence.Slice(offset);
        }

        /// <summary>
        /// Parses a string from the sequence buffer.
        /// </summary>
        private string ParseString(ReadOnlySequence<byte> sequence)
        {
            // Parses string fast from the single buffer Span<T> segment
            if (sequence.IsSingleSegment)
                return GetString(sequence.First.Span);

            // Parses string slow copying from multiple buffer Span<T> segments
            byte[] array = null;
            try
            {
                array = ArrayPool<byte>.Shared.Rent(checked((int)sequence.Length));
                var span = new Span<byte>(array, 0, (int)sequence.Length);
                sequence.CopyTo(span);
                return GetString(span);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(array);
            }
        }

        private string GetString(ReadOnlySpan<byte> span)
        {
            if (span.Length == 0)
                return null;

#if NETSTANDARD2_1
            return Encoding.UTF8.GetString(span);
#else
            unsafe
            {
                fixed (byte* ptr = span)
                {
                    return Encoding.UTF8.GetString(ptr, span.Length);
                }
            }
#endif
        }
    }
}
