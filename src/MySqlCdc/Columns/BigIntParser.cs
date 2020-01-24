using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class BigIntParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return reader.ReadLong(8);
        }
    }
}
