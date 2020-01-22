using System.Collections.Generic;

namespace MySqlCdc.Providers.MySql
{
    public class DefaultCharset
    {
        public int DefaultCharsetCollation { get; }
        public IReadOnlyDictionary<int, int> CharsetCollations { get; }

        public DefaultCharset(int defaultCharsetCollation, IReadOnlyDictionary<int, int> charsetCollations)
        {
            DefaultCharsetCollation = defaultCharsetCollation;
            CharsetCollations = charsetCollations;
        }
    }
}
