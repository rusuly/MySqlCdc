using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class StringParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            var length = metadata > 255 ? reader.ReadInt(2) : reader.ReadInt(1);
            return reader.ReadString(length);
        }
    }
}
