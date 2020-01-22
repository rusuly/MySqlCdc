namespace MySqlCdc.Providers.MySql
{
    internal enum MetadataType
    {
        SIGNEDNESS = 1,
        DEFAULT_CHARSET,
        COLUMN_CHARSET,
        COLUMN_NAME,
        SET_STR_VALUE,
        ENUM_STR_VALUE,
        GEOMETRY_TYPE,
        SIMPLE_PRIMARY_KEY,
        PRIMARY_KEY_WITH_PREFIX,
        ENUM_AND_SET_DEFAULT_CHARSET,
        ENUM_AND_SET_COLUMN_CHARSET
    }
}
