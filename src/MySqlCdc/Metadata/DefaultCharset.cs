namespace MySqlCdc.Metadata;

/// <summary>
/// Represents charsets of character columns.
/// </summary>
/// <remarks>
/// Creates a new <see cref="DefaultCharset"/>.
/// </remarks>
public record DefaultCharset(int DefaultCharsetCollation, IReadOnlyDictionary<int, int> CharsetCollations)
{
    /// <summary>
    /// Gets the most used charset collation.
    /// </summary>
    public int DefaultCharsetCollation { get; } = DefaultCharsetCollation;

    /// <summary>
    /// Gets ColumnIndex-Charset map for columns that don't use the default charset.
    /// </summary>
    public IReadOnlyDictionary<int, int> CharsetCollations { get; } = CharsetCollations;
}