using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class TimeStampParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            long seconds = reader.ReadLong(4);
            return DateTimeOffset.FromUnixTimeSeconds(seconds);
        }
    }
}
