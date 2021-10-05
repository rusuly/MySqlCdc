namespace MySqlCdc.Metadata;

internal enum MetadataType
{
    Signedness = 1,
    DefaultCharset = 2,
    ColumnCharset = 3,
    ColumnName = 4,
    SetStrValue = 5,
    EnumStrValue = 6,
    GeometryType = 7,
    SimplePrimaryKey = 8,
    PrimaryKeyWithPrefix = 9,
    EnumAndSetDefaultCharset = 10,
    EnumAndSetColumnCharset = 11,
    ColumnVisibility = 12
}