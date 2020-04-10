using System;
using System.Text.Json;
using MySqlCdc.Constants;

namespace MySqlCdc.Providers.MySql
{
    internal class JsonWriter : IJsonWriter
    {
        private readonly Utf8JsonWriter _writer;
        private string _propertyName;

        public JsonWriter(Utf8JsonWriter writer)
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
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
        }

        public void WriteValue(ushort value)
        {
            if (_propertyName != null)
            {
                _writer.WriteNumber(_propertyName, value);
                _propertyName = null;
            }
            else _writer.WriteNumberValue(value);
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

        public void WriteDate(DateTime value)
        {
            this.WriteValue(value.ToString("yyyy-MM-dd"));
        }

        public void WriteTime(TimeSpan value)
        {
            this.WriteValue(value.ToString("hh':'mm':'ss'.'fff"));
        }

        public void WriteDateTime(DateTime value)
        {
            this.WriteValue(value.ToString("yyyy-MM-dd HH:mm:ss.fff"));
        }

        public void WriteOpaque(ColumnType columnType, byte[] value)
        {
            this.WriteValue(Convert.ToBase64String(value));
        }
    }
}
