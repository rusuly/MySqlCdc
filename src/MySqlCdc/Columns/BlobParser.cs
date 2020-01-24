using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    internal class BlobParser : IColumnParser
    {
        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            var length = reader.ReadInt(metadata);
            return reader.ReadByteArraySlow(length);
        }
    }
}
