using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class SmallIntParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return (reader.ReadInt(2) << 16) >> 16;
        }
    }
}
