using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class IntParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return reader.ReadInt(4);
        }
    }
}
