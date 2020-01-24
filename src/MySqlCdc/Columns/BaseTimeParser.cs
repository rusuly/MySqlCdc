using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class BaseTimeParser
    {
        protected int ParseFractionalPart(ref PacketReader reader, int metadata)
        {
            int length = (metadata + 1) / 2;
            if (length == 0)
                return 0;

            int fraction = reader.ReadBigEndianInt(length);
            return fraction * (int)Math.Pow(100, 3 - length);
        }

        protected int GetBitSliceValue(long value, int startIndex, int length, int totalSize)
        {
            long result = value >> totalSize - (startIndex + length);
            return (int)(result & ((1 << length) - 1));
        }
    }
}
