using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class BitParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            int length = (metadata >> 8) * 8 + (metadata & 0xFF);
            return reader.ReadBitmapBigEndian(length);
        }
    }
}
