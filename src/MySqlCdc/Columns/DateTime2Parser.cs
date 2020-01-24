using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    //  See https://dev.mysql.com/doc/internals/en/date-and-time-data-type-representation.html
    //  1 bit  sign           (1 = non-negative, 0 = negative)
    // 17 bits year*13+month  (year 0-9999, month 0-12)
    //  5 bits day            (0-31)
    //  5 bits hour           (0-23)
    //  6 bits minute         (0-59)
    //  6 bits second         (0-59)
    //  ---------------------------
    // 40 bits = 5 bytes
    internal class DateTime2Parser : BaseTimeParser, IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadBigEndianLong(5);
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
    }
}
