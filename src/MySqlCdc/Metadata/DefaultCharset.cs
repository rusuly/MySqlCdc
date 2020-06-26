using System.Collections.Generic;

namespace MySqlCdc.Metadata
{
    /// <summary>
    /// Represents charsets of character columns.
    /// </summary>
    public class DefaultCharset
    {
        /// <summary>
        /// Gets the most used charset collation.
        /// </summary>
        public int DefaultCharsetCollation { get; }

        /// <summary>
        /// Gets ColumnIndex-Charset map for columns that don't use the default charset.
        /// </summary>
        public IReadOnlyDictionary<int, int> CharsetCollations { get; }

        /// <summary>
        /// Creates a new <see cref="DefaultCharset"/>.
        /// </summary>
        public DefaultCharset(int defaultCharsetCollation, IReadOnlyDictionary<int, int> charsetCollations)
        {
            DefaultCharsetCollation = defaultCharsetCollation;
            CharsetCollations = charsetCollations;
        }
    }
}
