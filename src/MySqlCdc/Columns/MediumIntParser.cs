using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class MediumIntParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            return (reader.ReadInt(3) << 8) >> 8;
        }
    }
}
