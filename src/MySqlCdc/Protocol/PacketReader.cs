using System;
using System.Buffers.Binary;
using System.Text;
using MySqlCdc.Constants;

namespace MySqlCdc.Protocol
{
    /// <summary>
    /// Constructs server reply from byte packet response.
    /// </summary>
    public ref struct PacketReader
    {
        private ReadOnlySpan<byte> _span;
        private int _offset;

        /// <summary>
        /// Creates a new <see cref="PacketReader"/>.
        /// </summary>
        public PacketReader(ReadOnlySpan<byte> span)
        {
            _span = span;
            _offset = 0;
        }

        /// <summary>
        /// Reads one byte as int number.
        /// </summary>
        public byte ReadByte() => _span[_offset++];

        /// <summary>
        /// Reads 16-bit int written in little-endian format.
        /// </summary>
        public UInt16 ReadUInt16LittleEndian()
        {
            UInt16 result = BinaryPrimitives.ReadUInt16LittleEndian(_span.Slice(_offset));
            _offset += 2;
            return result;
        }

        /// <summary>
        /// Reads 16-bit int written in big-endian format.
        /// </summary>
        public UInt16 ReadUInt16BigEndian()
        {
            UInt16 result = BinaryPrimitives.ReadUInt16BigEndian(_span.Slice(_offset));
            _offset += 2;
            return result;
        }

        /// <summary>
        /// Reads 32-bit int written in little-endian format.
        /// </summary>
        public int ReadInt32LittleEndian()
        {
            int result = BinaryPrimitives.ReadInt32LittleEndian(_span.Slice(_offset));
            _offset += 4;
            return result;
        }

        /// <summary>
        /// Reads 32-bit int written in big-endian format.
        /// </summary>
        public int ReadInt32BigEndian()
        {
            int result = BinaryPrimitives.ReadInt32BigEndian(_span.Slice(_offset));
            _offset += 4;
            return result;
        }

        /// <summary>
        /// Reads 64-bit long written in little-endian format.
        /// </summary>
        public long ReadInt64LittleEndian()
        {
            long result = BinaryPrimitives.ReadInt64LittleEndian(_span.Slice(_offset));
            _offset += 8;
            return result;
        }

        /// <summary>
        /// Reads int number written in little-endian format.
        /// </summary>
        public int ReadIntLittleEndian(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                result |= _span[_offset + i] << (i << 3);
            }
            _offset += length;
            return result;
        }

        /// <summary>
        /// Reads long number written in little-endian format.
        /// </summary>
        public long ReadLongLittleEndian(int length)
        {
            long result = 0;
            for (int i = 0; i < length; i++)
            {
                result |= (long)_span[_offset + i] << (i << 3);
            }
            _offset += length;
            return result;
        }

        /// <summary>
        /// Reads int number written in big-endian format.
        /// </summary>
        public int ReadIntBigEndian(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                result = (result << 8) | (int)_span[_offset + i];
            }
            _offset += length;
            return result;
        }

        /// <summary>
        /// Reads long number written in big-endian format.
        /// </summary>
        public long ReadLongBigEndian(int length)
        {
            long result = 0;
            for (int i = 0; i < length; i++)
            {
                result = (result << 8) | (long)_span[_offset + i];
            }
            _offset += length;
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
            byte firstByte = ReadByte();

            if (firstByte < 0xFB)
                return firstByte;
            else if (firstByte == 0xFB)
                throw new FormatException("Length encoded integer cannot be NULL.");
            else if (firstByte == 0xFC)
                return ReadUInt16LittleEndian();
            else if (firstByte == 0xFD)
                return ReadIntLittleEndian(3);
            else if (firstByte == 0xFE)
            {
                try
                {
                    // Max theoretical length of .NET string is Int32.MaxValue
                    return checked((int)ReadInt64LittleEndian());
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
            var span = _span.Slice(_offset, length);
            _offset += length;
            return ParseString(span);
        }

        /// <summary>
        /// Reads string to end of the sequence.
        /// </summary>
        public string ReadStringToEndOfFile()
        {
            var span = _span.Slice(_offset);
            _offset += span.Length;
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
                if (_span[_offset + index++] == PacketConstants.NullTerminator)
                    break;
            }
            var span = _span.Slice(_offset, index - 1);
            _offset += index;
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
            var span = _span.Slice(_offset, length);
            _offset += length;
            return span.ToArray();
        }

        /// <summary>
        /// Reads bitmap in little-endian bytes order
        /// </summary>
        public bool[] ReadBitmap(int bitsNumber)
        {
            var result = new bool[bitsNumber];
            var bytesNumber = (bitsNumber + 7) / 8;
            for (int i = 0; i < bytesNumber; i++)
            {
                byte value = _span[_offset + i];
                for (int y = 0; y < 8; y++)
                {
                    int index = (i << 3) + y;
                    if (index == bitsNumber)
                        break;
                    result[index] = (value & (1 << y)) > 0;
                }
            }
            _offset += bytesNumber;
            return result;
        }

        /// <summary>
        /// Reads bitmap in big-endian bytes order
        /// </summary>
        public bool[] ReadBitmapBigEndian(int bitsNumber)
        {
            var result = new bool[bitsNumber];
            var bytesNumber = (bitsNumber + 7) / 8;
            for (int i = 0; i < bytesNumber; i++)
            {
                byte value = _span[_offset + i];
                for (int y = 0; y < 8; y++)
                {
                    int index = ((bytesNumber - i - 1) << 3) + y;
                    if (index >= bitsNumber)
                        continue;
                    result[index] = (value & (1 << y)) > 0;
                }
            }
            _offset += bytesNumber;
            return result;
        }

        /// <summary>
        /// Checks whether the remaining buffer is empty
        /// </summary>
        public bool IsEmpty() => _span.Length == _offset;

        /// <summary>
        /// Gets number of consumed bytes
        /// </summary>
        public int Consumed => _offset;

        /// <summary>
        /// Skips the specified number of bytes in the buffer
        /// </summary>
        public void Advance(int offset) => _offset += offset;

        /// <summary>
        /// Removes the specified slice from the end
        /// </summary>
        public void SliceFromEnd(int length)
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
