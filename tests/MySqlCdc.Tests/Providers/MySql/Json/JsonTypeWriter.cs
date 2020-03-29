using System.Text.Json;

namespace MySqlCdc.Providers.MySql
{
    internal class JsonTypeWriter : IJsonWriter
    {
        private readonly Utf8JsonWriter _writer;
        private string _propertyName;

        public JsonTypeWriter(Utf8JsonWriter writer)
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

        public void WriteValue(short value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "Int16");
                _propertyName = null;
            }
            else _writer.WriteStringValue("Int16");
        }

        public void WriteValue(ushort value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "UInt16");
                _propertyName = null;
            }
            else _writer.WriteStringValue("UInt16");
        }

        public void WriteValue(int value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "Int32");
                _propertyName = null;
            }
            else _writer.WriteStringValue("Int32");
        }

        public void WriteValue(uint value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "UInt32");
                _propertyName = null;
            }
            else _writer.WriteStringValue("UInt32");
        }

        public void WriteValue(long value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "Int64");
                _propertyName = null;
            }
            else _writer.WriteStringValue("Int64");
        }

        public void WriteValue(ulong value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "UInt64");
                _propertyName = null;
            }
            else _writer.WriteStringValue("UInt64");
        }

        public void WriteValue(double value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "double");
                _propertyName = null;
            }
            else _writer.WriteStringValue("double");
        }

        public void WriteValue(string value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "string");
                _propertyName = null;
            }
            else _writer.WriteStringValue("string");
        }

        public void WriteValue(bool value)
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "bool");
                _propertyName = null;
            }
            else _writer.WriteStringValue("bool");
        }

        public void WriteNull()
        {
            if (_propertyName != null)
            {
                _writer.WriteString(_propertyName, "null");
                _propertyName = null;
            }
            else _writer.WriteStringValue("null");
        }
    }
}
