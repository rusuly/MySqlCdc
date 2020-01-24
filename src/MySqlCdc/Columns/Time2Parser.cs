using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    //  See https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html
    //  1 bit sign    (1 = non-negative, 0 = negative)
    //  1 bit unused  (reserved for future extensions)
    // 10 bits hour   (0-838)
    //  6 bits minute (0-59) 
    //  6 bits second (0-59) 
    //  ---------------------
    // 24 bits = 3 bytes
    internal class Time2Parser : BaseTimeParser, IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadBigEndianLong(3);
            int millisecond = ParseFractionalPart(ref reader, metadata) / 1000;

            int hours = GetBitSliceValue(value, 2, 10, 24);
            int minutes = GetBitSliceValue(value, 12, 6, 24);
            int seconds = GetBitSliceValue(value, 18, 6, 24);

            return new TimeSpan(0, hours, minutes, seconds, millisecond);
        }
    }
}
