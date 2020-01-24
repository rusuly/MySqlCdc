using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class SetParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return reader.ReadLong(metadata);
        }
    }
}
