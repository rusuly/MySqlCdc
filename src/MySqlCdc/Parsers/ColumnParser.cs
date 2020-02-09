using System;
using System.Buffers;
using System.Text;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class ColumnParser
    {
        private const int DigitsPerInt = 9;
        private static readonly int[] CompressedBytes = new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 4 };

        public object ParseNewDecimal(ref PacketReader reader, int metadata)
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
            var buffer = new PacketReader(memoryOwner.Memory);

            int size = CompressedBytes[compressedIntegral];
            if (size > 0)
            {
                result.Append(buffer.ReadIntBigEndian(size));
            }
            for (int i = 0; i < uncompressedIntegral; i++)
            {
                result.Append(buffer.ReadInt32BigEndian().ToString("D9"));
            }
            result.Append(".");

            size = CompressedBytes[compressedFractional];
            for (int i = 0; i < uncompressedFractional; i++)
            {
                result.Append(buffer.ReadInt32BigEndian().ToString("D9"));
            }
            if (size > 0)
            {
                result.Append(buffer.ReadIntBigEndian(size).ToString($"D{compressedFractional}"));
            }
            return result.ToString();
        }

        public object ParseBit(ref PacketReader reader, int metadata)
        {
            int length = (metadata >> 8) * 8 + (metadata & 0xFF);
            return reader.ReadBitmapBigEndian(length);
        }

        public object ParseTinyInt(ref PacketReader reader, int metadata)
        {
            return (reader.ReadByte() << 24) >> 24;
        }

        public object ParseSmallInt(ref PacketReader reader, int metadata)
        {
            return (reader.ReadInt16LittleEndian() << 16) >> 16;
        }

        public object ParseMediumInt(ref PacketReader reader, int metadata)
        {
            return (reader.ReadIntLittleEndian(3) << 8) >> 8;
        }

        public object ParseInt(ref PacketReader reader, int metadata)
        {
            return reader.ReadInt32LittleEndian();
        }

        public object ParseBigInt(ref PacketReader reader, int metadata)
        {
            return reader.ReadInt64LittleEndian();
        }

        public object ParseFloat(ref PacketReader reader, int metadata)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(reader.ReadInt32LittleEndian()), 0);
        }

        public object ParseDouble(ref PacketReader reader, int metadata)
        {
            return BitConverter.Int64BitsToDouble(reader.ReadInt64LittleEndian());
        }

        public object ParseString(ref PacketReader reader, int metadata)
        {
            var length = metadata > 255 ? reader.ReadInt16LittleEndian() : reader.ReadByte();
            return reader.ReadString(length);
        }

        public object ParseEnum(ref PacketReader reader, int metadata)
        {
            return reader.ReadIntLittleEndian(metadata);
        }

        public object ParseSet(ref PacketReader reader, int metadata)
        {
            return reader.ReadLongLittleEndian(metadata);
        }

        public object ParseYear(ref PacketReader reader, int metadata)
        {
            return 1900 + reader.ReadByte();
        }

        public object ParseDate(ref PacketReader reader, int metadata)
        {
            int value = reader.ReadIntLittleEndian(3);

            // Bits 1-5 store the day. 
            // Bits 6-9 store the month. 
            // The remaining bits store the year.
            int year = GetBitSliceValue(value, 0, 15, 24);
            int month = GetBitSliceValue(value, 15, 4, 24);
            int day = GetBitSliceValue(value, 19, 5, 24);

            if (year == 0 || month == 0 || day == 0)
                return null;

            return new DateTime(year, month, day);
        }

        public object ParseTime(ref PacketReader reader, int metadata)
        {
            int value = reader.ReadIntLittleEndian(3);
            int seconds = value % 100;
            value = value / 100;
            int minutes = value % 100;
            value = value / 100;
            int hours = value;
            return new TimeSpan(hours, minutes, seconds);
        }

        public object ParseTimeStamp(ref PacketReader reader, int metadata)
        {
            long seconds = (uint)reader.ReadInt32LittleEndian();
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }

        public object ParseDateTime(ref PacketReader reader, int metadata)
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

        //  See https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html
        //  1 bit sign    (1 = non-negative, 0 = negative)
        //  1 bit unused  (reserved for future extensions)
        // 10 bits hour   (0-838)
        //  6 bits minute (0-59) 
        //  6 bits second (0-59) 
        //  ---------------------
        // 24 bits = 3 bytes
        public object ParseTime2(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadLongBigEndian(3);
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;

            int hours = GetBitSliceValue(value, 2, 10, 24);
            int minutes = GetBitSliceValue(value, 12, 6, 24);
            int seconds = GetBitSliceValue(value, 18, 6, 24);

            return new TimeSpan(0, hours, minutes, seconds, millisecond);
        }

        public object ParseTimeStamp2(ref PacketReader reader, int metadata)
        {
            long seconds = (uint)reader.ReadInt32BigEndian();
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;
            long timestamp = seconds * 1000 + millisecond;

            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }

        //  See https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html
        //  1 bit  sign           (1 = non-negative, 0 = negative)
        // 17 bits year*13+month  (year 0-9999, month 0-12)
        //  5 bits day            (0-31)
        //  5 bits hour           (0-23)
        //  6 bits minute         (0-59)
        //  6 bits second         (0-59)
        //  ---------------------------
        // 40 bits = 5 bytes
        public object ParseDateTime2(ref PacketReader reader, int metadata)
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

        public object ParseBlob(ref PacketReader reader, int metadata)
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
