namespace MySqlCdc.Providers.MySql;

internal enum ValueType
{
    SMALL_OBJECT = 0x00,
    LARGE_OBJECT = 0x01,
    SMALL_ARRAY = 0x02,
    LARGE_ARRAY = 0x03,
    LITERAL = 0x04,
    INT16 = 0x05,
    UINT16 = 0x06,
    INT32 = 0x07,
    UINT32 = 0x08,
    INT64 = 0x09,
    UINT64 = 0x0a,
    DOUBLE = 0x0b,
    STRING = 0x0c,
    CUSTOM = 0x0f
}