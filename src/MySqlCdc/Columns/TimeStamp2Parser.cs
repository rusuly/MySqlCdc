using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class TimeStamp2Parser : BaseTimeParser, IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            long seconds = reader.ReadBigEndianLong(4);
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;
            long timestamp = seconds * 1000 + millisecond;

            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp);
        }
    }
}
