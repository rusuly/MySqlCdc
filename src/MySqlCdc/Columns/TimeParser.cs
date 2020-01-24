using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class TimeParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            int value = reader.ReadInt(3);
            int seconds = value % 100;
            value = value / 100;
            int minutes = value % 100;
            value = value / 100;
            int hours = value;
            return new TimeSpan(hours, minutes, seconds);
        }
    }
}
