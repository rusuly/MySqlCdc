using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class DateTimeParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            long value = reader.ReadLong(8);
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
    }
}
