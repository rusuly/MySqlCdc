using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class DoubleParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return BitConverter.ToDouble(BitConverter.GetBytes(reader.ReadLong(8)), 0);
        }
    }
}
