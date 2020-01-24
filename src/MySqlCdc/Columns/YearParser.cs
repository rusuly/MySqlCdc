using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class YearParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return 1900 + reader.ReadInt(1);
        }
    }
}
