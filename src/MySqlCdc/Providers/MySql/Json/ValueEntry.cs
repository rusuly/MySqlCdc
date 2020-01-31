namespace MySqlCdc.Providers.MySql
{
    internal class ValueEntry
    {
        public ValueType Type { get; }
        public object Value { get; }
        public int Offset { get; }
        public bool Inlined { get; }

        private ValueEntry(ValueType type, object value, int offset, bool inlined)
        {
            Type = type;
            Value = value;
            Offset = offset;
            Inlined = inlined;
        }

        public static ValueEntry FromInlined(ValueType type, object value)
        {
            return new ValueEntry(type, value, 0, true);
        }

        public static ValueEntry FromOffset(ValueType type, int offset)
        {
            return new ValueEntry(type, null, offset, false);
        }
    }
}
