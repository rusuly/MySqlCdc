using System.IO;
using System.Text;
using System.Text.Json;
using MySqlCdc.Providers.MySql;
using Newtonsoft.Json.Linq;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class JsonParserTests
    {
        static byte[] payload = new byte[] { 0, 13, 0, 222, 2, 95, 0, 5, 0, 100, 0, 5, 0, 105, 0, 5, 0, 110, 0, 6, 0, 116, 0, 6, 0, 122, 0, 6, 0, 128, 0, 6, 0, 134, 0, 6, 0, 140, 0, 10, 0, 150, 0, 11, 0, 161, 0, 11, 0, 172, 0, 11, 0, 183, 0, 12, 0, 5, 208, 138, 7, 195, 0, 9, 199, 0, 11, 207, 0, 12, 215, 0, 7, 222, 0, 9, 226, 0, 10, 234, 0, 2, 242, 0, 4, 0, 0, 4, 1, 0, 0, 156, 1, 4, 2, 0, 105, 110, 116, 49, 54, 105, 110, 116, 51, 50, 105, 110, 116, 54, 52, 100, 111, 117, 98, 108, 101, 115, 116, 114, 105, 110, 103, 117, 105, 110, 116, 49, 54, 117, 105, 110, 116, 51, 50, 117, 105, 110, 116, 54, 52, 115, 109, 97, 108, 108, 65, 114, 114, 97, 121, 108, 105, 116, 101, 114, 97, 108, 78, 117, 108, 108, 108, 105, 116, 101, 114, 97, 108, 84, 114, 117, 101, 115, 109, 97, 108, 108, 79, 98, 106, 101, 99, 116, 108, 105, 116, 101, 114, 97, 108, 70, 97, 108, 115, 101, 32, 108, 251, 255, 0, 162, 47, 77, 255, 255, 255, 255, 89, 164, 12, 220, 41, 140, 103, 65, 6, 77, 111, 110, 105, 99, 97, 80, 195, 0, 0, 0, 94, 208, 178, 0, 0, 0, 0, 0, 184, 100, 217, 69, 0, 0, 0, 13, 0, 170, 0, 0, 43, 0, 2, 103, 0, 4, 0, 0, 4, 1, 0, 4, 2, 0, 5, 208, 138, 7, 123, 0, 7, 127, 0, 9, 131, 0, 9, 139, 0, 10, 147, 0, 11, 155, 0, 12, 163, 0, 2, 0, 60, 0, 18, 0, 3, 0, 21, 0, 5, 0, 2, 26, 0, 2, 30, 0, 105, 100, 115, 117, 115, 101, 114, 115, 0, 0, 4, 0, 3, 0, 30, 0, 12, 13, 0, 12, 18, 0, 12, 24, 0, 4, 74, 111, 104, 110, 5, 74, 97, 109, 101, 115, 5, 83, 97, 114, 97, 104, 4, 0, 20, 0, 0, 16, 0, 5, 1, 0, 5, 2, 0, 5, 3, 0, 0, 0, 4, 0, 80, 195, 0, 0, 32, 108, 251, 255, 0, 94, 208, 178, 0, 0, 0, 0, 0, 162, 47, 77, 255, 255, 255, 255, 0, 184, 100, 217, 69, 0, 0, 0, 89, 164, 12, 220, 41, 140, 103, 65, 6, 77, 111, 110, 105, 99, 97, 13, 0, 66, 1, 95, 0, 5, 0, 100, 0, 5, 0, 105, 0, 5, 0, 110, 0, 6, 0, 116, 0, 6, 0, 122, 0, 6, 0, 128, 0, 6, 0, 134, 0, 6, 0, 140, 0, 10, 0, 150, 0, 11, 0, 161, 0, 11, 0, 172, 0, 11, 0, 183, 0, 12, 0, 5, 208, 138, 7, 195, 0, 9, 199, 0, 11, 207, 0, 12, 215, 0, 7, 222, 0, 9, 226, 0, 10, 234, 0, 2, 242, 0, 4, 0, 0, 4, 1, 0, 0, 6, 1, 4, 2, 0, 105, 110, 116, 49, 54, 105, 110, 116, 51, 50, 105, 110, 116, 54, 52, 100, 111, 117, 98, 108, 101, 115, 116, 114, 105, 110, 103, 117, 105, 110, 116, 49, 54, 117, 105, 110, 116, 51, 50, 117, 105, 110, 116, 54, 52, 115, 109, 97, 108, 108, 65, 114, 114, 97, 121, 108, 105, 116, 101, 114, 97, 108, 78, 117, 108, 108, 108, 105, 116, 101, 114, 97, 108, 84, 114, 117, 101, 115, 109, 97, 108, 108, 79, 98, 106, 101, 99, 116, 108, 105, 116, 101, 114, 97, 108, 70, 97, 108, 115, 101, 32, 108, 251, 255, 0, 162, 47, 77, 255, 255, 255, 255, 89, 164, 12, 220, 41, 140, 103, 65, 6, 77, 111, 110, 105, 99, 97, 80, 195, 0, 0, 0, 94, 208, 178, 0, 0, 0, 0, 0, 184, 100, 217, 69, 0, 0, 0, 4, 0, 20, 0, 0, 16, 0, 5, 1, 0, 5, 2, 0, 5, 3, 0, 0, 0, 4, 0, 2, 0, 60, 0, 18, 0, 3, 0, 21, 0, 5, 0, 2, 26, 0, 2, 30, 0, 105, 100, 115, 117, 115, 101, 114, 115, 0, 0, 4, 0, 3, 0, 30, 0, 12, 13, 0, 12, 18, 0, 12, 24, 0, 4, 74, 111, 104, 110, 5, 74, 97, 109, 101, 115, 5, 83, 97, 114, 97, 104 };

        [Fact]
        public void Test_JsonDocument_ReturnsJson()
        {
            var actualJson = JsonParser.Parse(payload);
            var expectedJson = File.ReadAllText("Providers/MySql/Json/value.json");

            var actualToken = JToken.Parse(actualJson);
            var expectedToken = JToken.Parse(expectedJson);
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_JsonDocument_ReturnsTypeTree()
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonParser.Parse(payload, new JsonTypeWriter(writer));
            }
            var actualTypes = Encoding.UTF8.GetString(stream.ToArray());
            var expectedTypes = File.ReadAllText("Providers/MySql/Json/types.json");

            var actualToken = JToken.Parse(actualTypes);
            var expectedToken = JToken.Parse(expectedTypes);
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_SmallObject_WithNestedObject()
        {
            byte[] payload = new byte[] { 0, 1, 0, 60, 0, 11, 0, 1, 0, 0, 12, 0, 97, 1, 0, 48, 0, 11, 0, 1, 0, 0, 12, 0, 98, 2, 0, 36, 0, 18, 0, 1, 0, 19, 0, 1, 0, 12, 20, 0, 2, 22, 0, 99, 101, 1, 100, 2, 0, 14, 0, 12, 10, 0, 12, 12, 0, 1, 102, 1, 103 };
            Assert.Equal("{\"a\":{\"b\":{\"c\":\"d\",\"e\":[\"f\",\"g\"]}}}", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_SmallArray_WithNestedArray()
        {
            byte[] payload = new byte[] { 2, 3, 0, 34, 0, 5, 255, 255, 2, 13, 0, 5, 1, 0, 2, 0, 21, 0, 12, 10, 0, 2, 12, 0, 1, 98, 1, 0, 9, 0, 12, 7, 0, 1, 99 };
            Assert.Equal("[-1,[\"b\",[\"c\"]],1]", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_SmallObject_WithEmptyKey()
        {
            byte[] payload = new byte[] { 0, 1, 0, 29, 0, 11, 0, 7, 0, 0, 18, 0, 98, 105, 116, 114, 97, 116, 101, 1, 0, 11, 0, 11, 0, 0, 0, 5, 0, 0 };
            Assert.Equal("{\"bitrate\":{\"\":0}}", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_SmallObject_Empty()
        {
            byte[] payload = new byte[] { 0, 0, 0, 4, 0 };
            Assert.Equal("{}", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_SmallArray_Empty()
        {
            byte[] payload = new byte[] { 2, 0, 0, 4, 0 };
            Assert.Equal("[]", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_SmallObject()
        {
            byte[] payload = new byte[] { 0, 19, 0, 89, 1, 137, 0, 3, 0, 140, 0, 3, 0, 143, 0, 4, 0, 147, 0, 4, 0, 151, 0, 4, 0, 155, 0, 6, 0, 161, 0, 6, 0, 167, 0, 6, 0, 173, 0, 7, 0, 180, 0, 7, 0, 187, 0, 7, 0, 194, 0, 8, 0, 202, 0, 8, 0, 210, 0, 8, 0, 218, 0, 12, 0, 230, 0, 12, 0, 242, 0, 12, 0, 254, 0, 13, 0, 11, 1, 13, 0, 5, 0, 0, 5, 1, 0, 5, 255, 255, 2, 24, 1, 0, 28, 1, 11, 32, 1, 4, 0, 0, 4, 1, 0, 5, 255, 127, 7, 40, 1, 4, 2, 0, 5, 0, 128, 7, 44, 1, 12, 48, 1, 7, 55, 1, 9, 59, 1, 2, 67, 1, 7, 77, 1, 9, 81, 1, 107, 46, 48, 107, 46, 49, 107, 46, 45, 49, 107, 46, 91, 93, 107, 46, 123, 125, 107, 46, 51, 46, 49, 52, 107, 46, 110, 117, 108, 108, 107, 46, 116, 114, 117, 101, 107, 46, 51, 50, 55, 54, 55, 107, 46, 51, 50, 55, 54, 56, 107, 46, 102, 97, 108, 115, 101, 107, 46, 45, 51, 50, 55, 54, 56, 107, 46, 45, 51, 50, 55, 54, 57, 107, 46, 115, 116, 114, 105, 110, 103, 107, 46, 50, 49, 52, 55, 52, 56, 51, 54, 52, 55, 107, 46, 50, 49, 52, 55, 52, 56, 51, 54, 52, 56, 107, 46, 116, 114, 117, 101, 95, 102, 97, 108, 115, 101, 107, 46, 45, 50, 49, 52, 55, 52, 56, 51, 54, 52, 56, 107, 46, 45, 50, 49, 52, 55, 52, 56, 51, 54, 52, 57, 0, 0, 4, 0, 0, 0, 4, 0, 31, 133, 235, 81, 184, 30, 9, 64, 0, 128, 0, 0, 255, 127, 255, 255, 6, 115, 116, 114, 105, 110, 103, 255, 255, 255, 127, 0, 0, 0, 128, 0, 0, 0, 0, 2, 0, 10, 0, 4, 1, 0, 4, 2, 0, 0, 0, 0, 128, 255, 255, 255, 127, 255, 255, 255, 255 };
            var expected = "{\"k.1\":1,\"k.0\":0,\"k.-1\":-1,\"k.true\":true,\"k.false\":false,\"k.null\":null,\"k.string\":\"string\",\"k.true_false\":[true,false],\"k.32767\":32767,\"k.32768\":32768,\"k.-32768\":-32768,\"k.-32769\":-32769,\"k.2147483647\":2147483647,\"k.2147483648\":2147483648,\"k.-2147483648\":-2147483648,\"k.-2147483649\":-2147483649,\"k.3.14\":3.14,\"k.{}\":{},\"k.[]\":[]}";

            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse(expected);
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_SmallArray()
        {
            byte[] payload = new byte[] { 2, 20, 0, 137, 0, 5, 255, 255, 5, 0, 0, 5, 1, 0, 4, 1, 0, 4, 2, 0, 4, 0, 0, 12, 64, 0, 2, 71, 0, 5, 255, 127, 7, 81, 0, 5, 0, 128, 7, 85, 0, 7, 89, 0, 9, 93, 0, 7, 101, 0, 9, 105, 0, 10, 113, 0, 11, 121, 0, 0, 129, 0, 2, 133, 0, 6, 115, 116, 114, 105, 110, 103, 2, 0, 10, 0, 4, 1, 0, 4, 2, 0, 0, 128, 0, 0, 255, 127, 255, 255, 255, 255, 255, 127, 0, 0, 0, 128, 0, 0, 0, 0, 0, 0, 0, 128, 255, 255, 255, 127, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 31, 133, 235, 81, 184, 30, 9, 64, 0, 0, 4, 0, 0, 0, 4, 0 };
            var expected = "[-1,0,1,true,false,null,\"string\",[true,false],32767,32768,-32768,-32769,2147483647,2147483648,-2147483648,-2147483649,18446744073709551615,3.14,{},[]]";
            Assert.Equal(expected, JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarLiteral_Null()
        {
            byte[] payload = new byte[] { 4, 0 };
            Assert.Equal("null", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarLiteral_True()
        {
            byte[] payload = new byte[] { 4, 1 };
            Assert.Equal("true", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarLiteral_False()
        {
            byte[] payload = new byte[] { 4, 2 };
            Assert.Equal("false", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarPositiveInt16()
        {
            byte[] payload = new byte[] { 5, 1, 0 };
            Assert.Equal("1", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarNegativeInt16()
        {
            byte[] payload = new byte[] { 5, 255, 255 };
            Assert.Equal("-1", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarMaxPositiveInt16()
        {
            byte[] payload = new byte[] { 5, 255, 127 };
            Assert.Equal("32767", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarMinNegativeInt16()
        {
            byte[] payload = new byte[] { 5, 0, 128 };
            Assert.Equal("-32768", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarPositiveInt32()
        {
            byte[] payload = new byte[] { 7, 0, 128, 0, 0 };
            Assert.Equal("32768", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarNegativeInt32()
        {
            byte[] payload = new byte[] { 7, 255, 127, 255, 255 };
            Assert.Equal("-32769", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarMaxPositiveInt32()
        {
            byte[] payload = new byte[] { 7, 255, 255, 255, 127 };
            Assert.Equal("2147483647", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarMinNegativeInt32()
        {
            byte[] payload = new byte[] { 7, 0, 0, 0, 128 };
            Assert.Equal("-2147483648", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarPositiveInt64()
        {
            byte[] payload = new byte[] { 9, 0, 0, 0, 128, 0, 0, 0, 0 };
            Assert.Equal("2147483648", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarNegativeInt64()
        {
            byte[] payload = new byte[] { 9, 255, 255, 255, 127, 255, 255, 255, 255 };
            Assert.Equal("-2147483649", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarMaxUInt64()
        {
            byte[] payload = new byte[] { 10, 255, 255, 255, 255, 255, 255, 255, 255 };
            Assert.Equal("18446744073709551615", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarDouble()
        {
            byte[] payload = new byte[] { 11, 89, 164, 12, 220, 40, 140, 103, 65 };
            Assert.Equal("12345670.87654321", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_ScalarString()
        {
            byte[] payload = new byte[] { 12, 26, 76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109, 32, 100, 111, 108, 111, 114, 32, 115, 105, 116, 32, 97, 109, 101, 116 };
            Assert.Equal("\"Lorem ipsum dolor sit amet\"", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_JsonSet_AdvancesPartialUpdate()
        {
            // {"id": 1, "name": "Monica", "types":[1,"123"]}
            // JSON_SET(columnName, '$.name', 'Mon')
            byte[] payload = new byte[] { 0, 3, 0, 57, 0, 25, 0, 2, 0, 27, 0, 4, 0, 31, 0, 5, 0, 5, 1, 0, 12, 36, 0, 2, 43, 0, 105, 100, 110, 97, 109, 101, 116, 121, 112, 101, 115, 3, 77, 111, 110, 105, 99, 97, 2, 0, 14, 0, 5, 1, 0, 12, 10, 0, 3, 49, 50, 51 };
            Assert.Equal("{\"id\":1,\"name\":\"Mon\",\"types\":[1,\"123\"]}", JsonParser.Parse(payload));
        }

        [Fact]
        public void Test_JsonRemove_AdvancesPartialUpdate()
        {
            // {"id": 1, "name": "Monica", "types":[1,"123"]}
            // JSON_REMOVE(columnName, '$.name')
            byte[] payload = new byte[] { 0, 2, 0, 57, 0, 25, 0, 2, 0, 31, 0, 5, 0, 5, 1, 0, 2, 43, 0, 0, 12, 36, 0, 2, 43, 0, 105, 100, 110, 97, 109, 101, 116, 121, 112, 101, 115, 6, 77, 111, 110, 105, 99, 97, 2, 0, 14, 0, 5, 1, 0, 12, 10, 0, 3, 49, 50, 51 };
            Assert.Equal("{\"id\":1,\"types\":[1,\"123\"]}", JsonParser.Parse(payload));
        }
    }
}
