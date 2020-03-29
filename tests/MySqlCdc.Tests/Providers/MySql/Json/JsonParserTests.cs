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
        public void Test_ParseBinaryJson_ReturnsJson()
        {
            var actualJson = JsonParser.Parse(payload);
            var expectedJson = File.ReadAllText("Providers/MySql/Json/value.json");

            var actualToken = JToken.Parse(actualJson);
            var expectedToken = JToken.Parse(expectedJson);
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_ParseBinaryJson_ReturnsTypeTree()
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
        public void Test_JsonObject_WithNestedObject()
        {
            byte[] payload = new byte[] { 0, 1, 0, 60, 0, 11, 0, 1, 0, 0, 12, 0, 97, 1, 0, 48, 0, 11, 0, 1, 0, 0, 12, 0, 98, 2, 0, 36, 0, 18, 0, 1, 0, 19, 0, 1, 0, 12, 20, 0, 2, 22, 0, 99, 101, 1, 100, 2, 0, 14, 0, 12, 10, 0, 12, 12, 0, 1, 102, 1, 103 };
            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse("{\"a\":{\"b\":{\"c\":\"d\",\"e\":[\"f\",\"g\"]}}}");
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }


        [Fact]
        public void Test_JsonArray_WithNestedArray()
        {
            byte[] payload = new byte[] { 2, 3, 0, 34, 0, 5, 255, 255, 2, 13, 0, 5, 1, 0, 2, 0, 21, 0, 12, 10, 0, 2, 12, 0, 1, 98, 1, 0, 9, 0, 12, 7, 0, 1, 99 };
            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse("[-1,[\"b\",[\"c\"]],1]");
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_JsonObject_WithEmptyKey()
        {
            byte[] payload = new byte[] { 0, 1, 0, 29, 0, 11, 0, 7, 0, 0, 18, 0, 98, 105, 116, 114, 97, 116, 101, 1, 0, 11, 0, 11, 0, 0, 0, 5, 0, 0 };
            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse("{\"bitrate\":{\"\":0}}");
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_ScalarLiteral_Null()
        {
            byte[] payload = new byte[] { 4, 0 };
            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse("null");
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_ScalarLiteral_True()
        {
            byte[] payload = new byte[] { 4, 1 };
            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse("true");
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }

        [Fact]
        public void Test_ScalarLiteral_False()
        {
            byte[] payload = new byte[] { 4, 2 };
            var actualToken = JToken.Parse(JsonParser.Parse(payload));
            var expectedToken = JToken.Parse("false");
            Assert.True(JToken.DeepEquals(actualToken, expectedToken));
        }
    }
}
