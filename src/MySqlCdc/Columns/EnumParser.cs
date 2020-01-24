using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class EnumParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return reader.ReadInt(metadata);
        }
    }
}
