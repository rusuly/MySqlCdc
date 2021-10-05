using MySqlCdc.Constants;
using MySqlCdc.Parsers;
using Xunit;

namespace MySqlCdc.Tests.Providers;

public class RowEventParserTests
{
    [Fact]
    public void Test_GetActualStringType_CHAR()
    {
        // char(200)
        int columnType = (int)ColumnType.String;
        int metadata = 52768;
        RowEventParser.GetActualStringType(ref columnType, ref metadata);

        Assert.Equal((int)ColumnType.String, columnType);
        Assert.Equal(800 /* 200*Utf8Mb4 */ , metadata);
    }

    [Fact]
    public void Test_GetActualStringType_ENUM()
    {
        // enum('Low', 'Medium', 'High')
        int columnType = (int)ColumnType.String;
        int metadata = 63233;
        RowEventParser.GetActualStringType(ref columnType, ref metadata);

        Assert.Equal((int)ColumnType.Enum, columnType);
        Assert.Equal(1, metadata);
    }

    [Fact]
    public void Test_GetActualStringType_SET()
    {
        // set('Green', 'Yellow', 'Red')
        int columnType = (int)ColumnType.String;
        int metadata = 63489;
        RowEventParser.GetActualStringType(ref columnType, ref metadata);

        Assert.Equal((int)ColumnType.Set, columnType);
        Assert.Equal(1, metadata);
    }
}