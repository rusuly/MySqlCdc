using Newtonsoft.Json;

namespace MySqlCdc.Providers.MySql
{
    internal class JsonWriterImpl : IJsonWriter
    {
        private readonly JsonWriter _writer;

        public JsonWriterImpl(JsonWriter writer)
        {
            _writer = writer;
        }

        public void WriteKey(string name)
        {
            _writer.WritePropertyName(name);
        }

        public void WriteStartObject()
        {
            _writer.WriteStartObject();
        }

        public void WriteStartArray()
        {
            _writer.WriteStartArray();
        }

        public void WriteEndObject()
        {
            _writer.WriteEndObject();
        }

        public void WriteEndArray()
        {
            _writer.WriteEndArray();
        }

        public void WriteValue(int value)
        {
            _writer.WriteValue(value);
        }

        public void WriteValue(uint value)
        {
            _writer.WriteValue(value);
        }

        public void WriteValue(long value)
        {
            _writer.WriteValue(value);
        }

        public void WriteValue(ulong value)
        {
            _writer.WriteValue(value);
        }

        public void WriteValue(double value)
        {
            _writer.WriteValue(value);
        }

        public void WriteValue(string value)
        {
            _writer.WriteValue(value);
        }

        public void WriteValue(bool value)
        {
            _writer.WriteValue(value);
        }

        public void WriteNull()
        {
            _writer.WriteNull();
        }
    }
}
