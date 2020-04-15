using System;
using System.Buffers;
using System.Text;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    /// <summary>
    /// See <a href="https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html">Docs</a>
    /// </summary>
    internal class ColumnParser
    {
        private const int DigitsPerInt = 9;
        private static readonly int[] CompressedBytes = new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 4 };

        public string ParseNewDecimal(ref PacketReader reader, int metadata)
        {
            int precision = metadata & 0xFF;
            int scale = metadata >> 8;
            int integral = precision - scale;

            int uncompressedIntegral = integral / DigitsPerInt;
            int uncompressedFractional = scale / DigitsPerInt;
            int compressedIntegral = integral - (uncompressedIntegral * DigitsPerInt);
            int compressedFractional = scale - (uncompressedFractional * DigitsPerInt);
            int length =
                (uncompressedIntegral << 2) + CompressedBytes[compressedIntegral] +
                (uncompressedFractional << 2) + CompressedBytes[compressedFractional];

            byte[] value = reader.ReadByteArraySlow(length);
            var result = new StringBuilder();

            bool negative = (value[0] & 0x80) == 0;
            value[0] ^= 0x80;

            if (negative)
            {
                result.Append("-");
                for (int i = 0; i < value.Length; i++)
                    value[i] ^= 0xFF;
            }

            using var memoryOwner = new MemoryOwner(new ReadOnlySequence<byte>(value));
            var buffer = new PacketReader(memoryOwner.Memory.Span);

            int size = CompressedBytes[compressedIntegral];
            if (size > 0)
            {
                result.Append(buffer.ReadIntBigEndian(size));
            }
            for (int i = 0; i < uncompressedIntegral; i++)
            {
                result.Append(buffer.ReadUInt32BigEndian().ToString("D9"));
            }
            result.Append(".");

            size = CompressedBytes[compressedFractional];
            for (int i = 0; i < uncompressedFractional; i++)
            {
                result.Append(buffer.ReadUInt32BigEndian().ToString("D9"));
            }
            if (size > 0)
            {
                result.Append(buffer.ReadIntBigEndian(size).ToString($"D{compressedFractional}"));
            }
            return result.ToString();
        }

        public byte ParseTinyInt(ref PacketReader reader, int metadata) => reader.ReadByte();

        public Int16 ParseSmallInt(ref PacketReader reader, int metadata) => (Int16)reader.ReadUInt16LittleEndian();

        public Int32 ParseMediumInt(ref PacketReader reader, int metadata) => (reader.ReadIntLittleEndian(3) << 8) >> 8;

        public Int32 ParseInt(ref PacketReader reader, int metadata) => (Int32)reader.ReadUInt32LittleEndian();

        public Int64 ParseBigInt(ref PacketReader reader, int metadata) => reader.ReadInt64LittleEndian();

        public float ParseFloat(ref PacketReader reader, int metadata)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(reader.ReadUInt32LittleEndian()), 0);
        }

        public double ParseDouble(ref PacketReader reader, int metadata)
        {
            return BitConverter.Int64BitsToDouble(reader.ReadInt64LittleEndian());
        }

        public string ParseString(ref PacketReader reader, int metadata)
        {
            int length = metadata > 255 ? reader.ReadUInt16LittleEndian() : reader.ReadByte();
            return reader.ReadString(length);
        }

        public bool[] ParseBit(ref PacketReader reader, int metadata)
        {
            int length = (metadata >> 8) * 8 + (metadata & 0xFF);
            return reader.ReadBitmapBigEndian(length);
        }

        public int ParseEnum(ref PacketReader reader, int metadata)
        {
            return reader.ReadIntLittleEndian(metadata);
        }

        public long ParseSet(ref PacketReader reader, int metadata)
        {
            return reader.ReadLongLittleEndian(metadata);
        }

        public int ParseYear(ref PacketReader reader, int metadata)
        {
            return 1900 + (int)reader.ReadByte();
        }

        public DateTime? ParseDate(ref PacketReader reader, int metadata)
        {
            int value = reader.ReadIntLittleEndian(3);

            int year = GetBitSliceValue(value, 0, 15, 24);
            int month = GetBitSliceValue(value, 15, 4, 24);
            int day = GetBitSliceValue(value, 19, 5, 24);

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day);
        }

        public TimeSpan ParseTime(ref PacketReader reader, int metadata)
        {
            int value = reader.ReadIntLittleEndian(3);
            int seconds = value % 100;
            value = value / 100;
            int minutes = value % 100;
            value = value / 100;
            int hours = value;
            return new TimeSpan(hours, minutes, seconds);
        }

        public DateTimeOffset ParseTimeStamp(ref PacketReader reader, int metadata)
        {
            long seconds = reader.ReadUInt32LittleEndian();
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        public DateTime? ParseDateTime(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadInt64LittleEndian();
            int second = (int)value % 100;
            value = value / 100;
            int minute = (int)value % 100;
            value = value / 100;
            int hour = (int)value % 100;
            value = value / 100;
            int day = (int)value % 100;
            value = value / 100;
            int month = (int)value % 100;
            value = value / 100;
            int year = (int)value;

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day, hour, minute, second);
        }

        public TimeSpan ParseTime2(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadLongBigEndian(3);
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;

            bool negative = ((value >> 23) & 1) == 0;
            if (negative)
                throw new NotSupportedException("Time columns with negative values are not supported in this version");

            int hour = (int)(value >> 12) % (1 << 10);
            int minute = (int)(value >> 6) % (1 << 6);
            int second = (int)value % (1 << 6);
            return new TimeSpan(0, hour, minute, second, millisecond);
        }

        public DateTimeOffset ParseTimeStamp2(ref PacketReader reader, int metadata)
        {
            long seconds = reader.ReadUInt32BigEndian();
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;
            long timestamp = seconds * 1000 + millisecond;

            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }

        public DateTime? ParseDateTime2(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadLongBigEndian(5);
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;

            int yearMonth = GetBitSliceValue(value, 1, 17, 40);
            int year = yearMonth / 13;
            int month = yearMonth % 13;
            int day = GetBitSliceValue(value, 18, 5, 40);
            int hour = GetBitSliceValue(value, 23, 5, 40);
            int minute = GetBitSliceValue(value, 28, 6, 40);
            int second = GetBitSliceValue(value, 34, 6, 40);

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        public byte[] ParseBlob(ref PacketReader reader, int metadata)
        {
            var length = reader.ReadIntLittleEndian(metadata);
            return reader.ReadByteArraySlow(length);
        }

        private int ParseFractionalPart(ref PacketReader reader, int metadata)
        {
            int length = (metadata + 1) / 2;
            if (length == 0)
                return 0;

            int fraction = reader.ReadIntBigEndian(length);
            return fraction * (int)Math.Pow(100, 3 - length);
        }

        private int GetBitSliceValue(long value, int startIndex, int length, int totalSize)
        {
            long result = value >> totalSize - (startIndex + length);
            return (int)(result & ((1 << length) - 1));
        }
    }
}
