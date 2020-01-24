using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class DateParser : BaseTimeParser, IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            int value = reader.ReadInt(3);

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
    }
}
