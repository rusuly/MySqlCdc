using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class FloatParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return BitConverter.ToSingle(BitConverter.GetBytes(reader.ReadInt(4)), 0);
        }
    }
}
