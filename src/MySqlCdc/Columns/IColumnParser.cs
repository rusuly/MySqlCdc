using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    public interface IColumnParser
    {
        object ParseColumn(ref PacketReader reader, int metadata);
    }
}
