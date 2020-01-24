using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class TinyIntParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return (reader.ReadInt(1) << 24) >> 24;
        }
    }
}
