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
    public ref struct PacketReader
    {
        private SequenceReader<byte> _reader;

        /// <summary>
        /// Creates a new <see cref="PacketReader"/>.
        /// </summary>
        public PacketReader(ReadOnlySequence<byte> sequence)
        {
            _reader = new SequenceReader<byte>(sequence);
        }

        /// <summary>
        /// Reads int number written in little-endian format.
        /// </summary>
        public int ReadInt(int length)
        {
            int result = 0;
            for (int i = 0; i < length; i++)
            {
                _reader.TryRead(out byte value);
                result |= value << (i << 3);
            }
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
                _reader.TryRead(out byte value);
                result |= (long)value << (i << 3);
            }
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
                _reader.TryRead(out byte value);
                result = (result << 8) | (int)value;
            }
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
                _reader.TryRead(out byte value);
                result = (result << 8) | (long)value;
            }
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
            var sequence = _reader.Sequence.Slice(_reader.Consumed, length);
            _reader.Advance(length);
            return ParseString(sequence);
        }

        /// <summary>
        /// Reads string to end of the sequence.
        /// </summary>
        public string ReadStringToEndOfFile()
        {
            var sequence = _reader.Sequence.Slice(_reader.Consumed);
            _reader.Advance(_reader.Remaining);
            return ParseString(sequence);
        }

        /// <summary>
        /// Reads string terminated by 0 byte.
        /// </summary>
        public string ReadNullTerminatedString()
        {
            _reader.TryReadTo(out ReadOnlySequence<byte> sequence, PacketConstants.NullTerminator);
            return ParseString(sequence);
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
            var sequence = _reader.Sequence.Slice(_reader.Consumed, length);
            _reader.Advance(length);
            return sequence.ToArray();
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
                _reader.TryRead(out byte value);
                for (int y = 0; y < 8; y++)
                {
                    int index = (i << 3) + y;
                    if (index == bitsNumber)
                        break;
                    result[index] = (value & (1 << y)) > 0;
                }
            }
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
                _reader.TryRead(out byte value);
                for (int y = 0; y < 8; y++)
                {
                    int index = ((bytesNumber - i - 1) << 3) + y;
                    if (index >= bitsNumber)
                        continue;
                    result[index] = (value & (1 << y)) > 0;
                }
            }
            return result;
        }

        /// <summary>
        /// Checks whether the remaining buffer is empty
        /// </summary>
        public bool IsEmpty() => _reader.Remaining == 0;

        /// <summary>
        /// Gets number of consumed bytes
        /// </summary>
        public int Consumed => (int)_reader.Consumed;

        /// <summary>
        /// Skips the specified number of bytes in the buffer
        /// </summary>
        public void Skip(int offset) => _reader.Advance(offset);

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
