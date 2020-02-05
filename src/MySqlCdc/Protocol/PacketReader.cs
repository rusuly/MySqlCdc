using System;
using System.Collections;
using System.Text;
using MySqlCdc.Constants;

namespace MySqlCdc.Protocol
{
    /// <summary>
    /// Constructs server reply from byte packet response.
    /// </summary>
    public ref struct PacketReader
    {
        private int _consumed; // Used separatly from property to improve performance
        private ReadOnlySpan<byte> _span;

        /// <summary>
        /// Creates a new <see cref="PacketReader"/>.
        /// </summary>
        public PacketReader(ReadOnlyMemory<byte> memory)
        {
            _span = memory.Span;
            _consumed = 0;
        }

        /// <summary>
        /// Reads int number written in little-endian format.
        /// </summary>
        public int ReadInt(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                result |= _span[_consumed + i] << (i << 3);
            }
            Skip(length);
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
                result |= (long)_span[_consumed + i] << (i << 3);
            }
            Skip(length);
            return result;
        }

        /// <summary>
        /// Reads int number written in big-endian format.
        /// </summary>
        public int ReadBigEndianInt(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                result = (result << 8) | (int)_span[_consumed + i];
            }
            Skip(length);
            return result;
        }

        /// <summary>
        /// Reads long number written in big-endian format.
        /// </summary>
        public long ReadBigEndianLong(int length)
        {
            long result = 0;
            for (int i = 0; i < length; i++)
            {
                result = (result << 8) | (long)_span[_consumed + i];
            }
            Skip(length);
            return result;
        }

        /// <summary>
        /// if first byte is less than 0xFB - Integer value is this 1 byte integer
        /// 0xFB - NULL value
        /// 0xFC - Integer value is encoded in the next 2 bytes (3 bytes total)
        /// 0xFD - Integer value is encoded in the next 3 bytes (4 bytes total)
        /// 0xFE - Integer value is encoded in the next 8 bytes (9 bytes total)
        /// </summary>
        public int ReadLengthEncodedNumber()
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
            var span = _span.Slice(_consumed, length);
            Skip(length);
            return ParseString(span);
        }

        /// <summary>
        /// Reads string to end of the sequence.
        /// </summary>
        public string ReadStringToEndOfFile()
        {
            var span = _span.Slice(_consumed);
            Skip(span.Length);
            return ParseString(span);
        }

        /// <summary>
        /// Reads string terminated by 0 byte.
        /// </summary>
        public string ReadNullTerminatedString()
        {
            int index = 0;
            while (true)
            {
                if (_span[_consumed + index++] == PacketConstants.NullTerminator)
                    break;
            }
            var span = _span.Slice(_consumed, index - 1);
            Skip(index);
            return ParseString(span);
        }

        /// <summary>
        /// Reads length-encoded string.
        /// </summary>
        public string ReadLengthEncodedString()
        {
            var length = ReadLengthEncodedNumber();
            return ReadString(length);
        }

        /// <summary>
        /// Reads byte array from the sequence.
        /// Allocates managed memory for the array.
        /// </summary>
        public byte[] ReadByteArraySlow(int length)
        {
            var span = _span.Slice(_consumed, length);
            Skip(span.Length);
            return span.ToArray();
        }

        /// <summary>
        /// Reads bitmap in little-endian bytes order
        /// </summary>
        public BitArray ReadBitmap(int bitsNumber)
        {
            var result = new BitArray(bitsNumber);
            var bytesNumber = (bitsNumber + 7) / 8;
            for (int i = 0; i < bytesNumber; i++)
            {
                byte value = _span[_consumed + i];
                for (int y = 0; y < 8; y++)
                {
                    int index = (i << 3) + y;
                    if (index == bitsNumber)
                        break;
                    result[index] = (value & (1 << y)) > 0;
                }
            }
            Skip(bytesNumber);
            return result;
        }

        /// <summary>
        /// Reads bitmap in big-endian bytes order
        /// </summary>
        public BitArray ReadBitmapBigEndian(int bitsNumber)
        {
            var result = new BitArray(bitsNumber);
            var bytesNumber = (bitsNumber + 7) / 8;
            for (int i = 0; i < bytesNumber; i++)
            {
                byte value = _span[_consumed + i];
                for (int y = 0; y < 8; y++)
                {
                    int index = ((bytesNumber - i - 1) << 3) + y;
                    if (index >= bitsNumber)
                        continue;
                    result[index] = (value & (1 << y)) > 0;
                }
            }
            Skip(bytesNumber);
            return result;
        }

        /// <summary>
        /// Checks whether the remaining buffer is empty
        /// </summary>
        public bool IsEmpty() => _span.Length == _consumed;

        /// <summary>
        /// Gets number of consumed bytes
        /// </summary>
        public int Consumed => _consumed;

        /// <summary>
        /// Skips the specified number of bytes in the buffer
        /// </summary>
        public void Skip(int offset) => _consumed += offset;

        public void SliceFromEnd(int index, int length)
        {
            _span = _span.Slice(0, _span.Length - length);
        }

        /// <summary>
        /// Parses a string from the span.
        /// </summary>
        private string ParseString(ReadOnlySpan<byte> span)
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
