using System.Text.Json;

namespace MySqlCdc.Providers.MySql
{
    internal class JsonWriterImpl : IJsonWriter
    {
        private readonly Utf8JsonWriter _writer;
        private string _propertyName;

        public JsonWriterImpl(Utf8JsonWriter writer)
        {
            _writer = writer;
        }

        public void WriteKey(string name)
        {
            _propertyName = name;
        }

        public void WriteStartObject()
        {
            if (_propertyName != null)
            {
                _writer.WriteStartObject(_propertyName);
                _propertyName = null;
            }
            else _writer.WriteStartObject();
        }

        public void WriteStartArray()
        {
            if (_propertyName != null)
            {
                _writer.WriteStartArray(_propertyName);
                _propertyName = null;
            }
            else _writer.WriteStartArray();
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
            if (_propertyName != null)
            {
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
        }

        public void WriteValue(uint value)
        {
            if (_propertyName != null)
            {
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
        }

        public void WriteValue(long value)
        {
            if (_propertyName != null)
            {
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
        }

        public void WriteValue(ulong value)
        {
            if (_propertyName != null)
            {
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
        }

        public void WriteValue(double value)
        {
            if (_propertyName != null)
            {
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
        }

        public void WriteValue(string value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteStringValue(value);
        }

        public void WriteValue(bool value)
        {
            if (_propertyName != null)
            {
                _writer.WriteBoolean(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteBooleanValue(value);
        }

        public void WriteNull()
        {
            if (_propertyName != null)
            {
                _writer.WriteNull(_propertyName);
                _propertyName = null;
            }
            else _writer.WriteNullValue();
        }
    }
}
