using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    /// <summary>
    /// Base interface for column type parsers.
    /// </summary>
    public interface IColumnParser
    {
        /// <summary>
        /// Parses column value using the provided metadata.
        /// </summary>
        object ParseColumn(ref PacketReader reader, int metadata);
    }
}
