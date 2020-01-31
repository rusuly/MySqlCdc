using System;
using System.Buffers;
using System.IO;
using System.Text;
using MySqlCdc.Columns;
using MySqlCdc.Protocol;
using Newtonsoft.Json;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// Parses json object stored in MySQL binary format.
    /// See <a href="https://dev.mysql.com/worklog/task/?id=8132">specification</a>
    /// See <a href="https://github.com/shyiko/mysql-binlog-connector-java">JsonBinary</a>
    /// See <a href="https://github.com/mysql/mysql-server/blob/8.0/sql/json_binary.cc">json_binary.cc</a>
    /// See <a href="https://github.com/mysql/mysql-server/blob/8.0/sql/json_binary.h">json_binary.h</a>
    /// </summary>
    public sealed class JsonParser
    {
        private readonly static DoubleParser DoubleParser = new DoubleParser();

        /// <summary>
        /// Parses MySQL binary format json to a string.
        /// </summary>
        public static string Parse(byte[] data)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb);

            using var writer = new JsonTextWriter(sw);
            JsonParser.Parse(data, new JsonWriterImpl(writer));
            return sb.ToString();
        }

        /// <summary>
        /// Parses MySQL binary format using the provided writer.
        /// </summary>
        public static void Parse(byte[] data, IJsonWriter writer)
        {
            var parser = new JsonParser(writer);
            var reader = new PacketReader(new ReadOnlySequence<byte>(data));
            var valueType = parser.ReadValueType(ref reader);
            parser.ParseNode(ref reader, valueType);
        }

        private readonly IJsonWriter _writer;

        private JsonParser(IJsonWriter writer)
        {
            _writer = writer;
        }

        private ValueType ReadValueType(ref PacketReader reader) => (ValueType)reader.ReadInt(1);

        private void ParseNode(ref PacketReader reader, ValueType valueType)
        {
            switch (valueType)
            {
                case ValueType.SMALL_OBJECT:
                    ParseArrayOrObject(ref reader, valueType, true, true);
                    break;
                case ValueType.LARGE_OBJECT:
                    ParseArrayOrObject(ref reader, valueType, false, true);
                    break;
                case ValueType.SMALL_ARRAY:
                    ParseArrayOrObject(ref reader, valueType, true, false);
                    break;
                case ValueType.LARGE_ARRAY:
                    ParseArrayOrObject(ref reader, valueType, false, false);
                    break;
                case ValueType.LITERAL:
                    ParseLiteral(ref reader);
                    break;
                case ValueType.INT16:
                    _writer.WriteValue((reader.ReadInt(2) << 16) >> 16);
                    break;
                case ValueType.UINT16:
                    _writer.WriteValue(reader.ReadInt(2));
                    break;
                case ValueType.INT32:
                    _writer.WriteValue(reader.ReadInt(4));
                    break;
                case ValueType.UINT32:
                    _writer.WriteValue((uint)reader.ReadInt(4));
                    break;
                case ValueType.INT64:
                    _writer.WriteValue(reader.ReadLong(8));
                    break;
                case ValueType.UINT64:
                    _writer.WriteValue((ulong)reader.ReadLong(8));
                    break;
                case ValueType.DOUBLE:
                    _writer.WriteValue((double)DoubleParser.ParseColumn(ref reader, 0));
                    break;
                case ValueType.STRING:
                    ParseString(ref reader);
                    break;
                case ValueType.CUSTOM:
                    throw new NotSupportedException($"Parsing JSON opaque types is not supported");
                default:
                    throw new NotSupportedException($"Unknown JSON value type {valueType}");
            }
        }

        /// <summary>
        /// Skips empty holes made by partial updates (MySQL 8.0.16+)
        /// </summary>
        private void Advance(ref PacketReader reader, int startIndex, int offset)
        {
            int holeSize = offset - (reader.Consumed - startIndex);
            if (holeSize > 0)
                reader.Skip(holeSize);
        }

        private void ParseArrayOrObject(ref PacketReader reader, ValueType valueType, bool small, bool isObject)
        {
            int startIndex = reader.Consumed;
            int valueSize = small ? 2 : 4;
            int elementsNumber = ReadJsonSize(ref reader, small);
            int bytesNumber = ReadJsonSize(ref reader, small);

            // Key entries
            int[] keyOffset = isObject ? new int[elementsNumber] : null;
            int[] keyLength = isObject ? new int[elementsNumber] : null;
            if (isObject)
            {
                for (int i = 0; i < elementsNumber; i++)
                {
                    keyOffset[i] = ReadJsonSize(ref reader, small);
                    keyLength[i] = reader.ReadInt(2);
                }
            }

            // Value entries
            var entries = new ValueEntry[elementsNumber];
            for (int i = 0; i < elementsNumber; i++)
            {
                ValueType type = ReadValueType(ref reader);
                if (type == ValueType.LITERAL)
                {
                    entries[i] = ValueEntry.FromInlined(type, ReadLiteral(ref reader));
                    reader.Skip(valueSize - 1);
                }
                else if (type == ValueType.INT16)
                {
                    entries[i] = ValueEntry.FromInlined(type, (reader.ReadInt(2) << 16) >> 16);
                    reader.Skip(valueSize - 2);
                }
                else if (type == ValueType.UINT16)
                {
                    entries[i] = ValueEntry.FromInlined(type, reader.ReadInt(2));
                    reader.Skip(valueSize - 2);
                }
                else if (type == ValueType.INT32 && !small)
                {
                    entries[i] = ValueEntry.FromInlined(type, reader.ReadInt(4));
                }
                else if (type == ValueType.UINT32 && !small)
                {
                    entries[i] = ValueEntry.FromInlined(type, (uint)reader.ReadInt(4));
                }
                else
                {
                    int offset = ReadJsonSize(ref reader, small);
                    if (offset >= bytesNumber)
                        throw new FormatException("The offset in JSON value was too long");
                    entries[i] = ValueEntry.FromOffset(type, offset);
                }
            }

            // Key rows
            string[] keys = null;
            if (isObject)
            {
                keys = new string[elementsNumber];
                for (int i = 0; i < elementsNumber; i++)
                {
                    // 1 - Remove a hole between keys
                    Advance(ref reader, startIndex, keyOffset[i]);
                    keys[i] = reader.ReadString(keyLength[i]);
                }
            }

            // Value rows
            if (isObject) _writer.WriteStartObject(); else _writer.WriteStartArray();
            for (int i = 0; i < elementsNumber; i++)
            {
                if (isObject) _writer.WriteKey(keys[i]);

                ValueEntry entry = entries[i];
                if (!entry.Inlined)
                {
                    // 2 - Remove a hole between values
                    Advance(ref reader, startIndex, entry.Offset);
                    ParseNode(ref reader, entry.Type);
                }
                else if (entry.Value == null)
                    _writer.WriteNull();
                else if (entry.Type == ValueType.LITERAL)
                    _writer.WriteValue((bool)entry.Value);
                else if (entry.Type == ValueType.UINT32)
                    _writer.WriteValue((uint)entry.Value);
                else
                    _writer.WriteValue((int)entry.Value);
            }
            if (isObject) _writer.WriteEndObject(); else _writer.WriteEndArray();

            // 3 - Remove a hole from the end
            Advance(ref reader, startIndex, bytesNumber);
        }

        private void ParseLiteral(ref PacketReader reader)
        {
            bool? literal = ReadLiteral(ref reader);
            if (literal == null)
                _writer.WriteNull();
            else
                _writer.WriteValue(literal.Value);
        }

        private void ParseString(ref PacketReader reader)
        {
            int length = ReadDataLength(ref reader);
            string value = reader.ReadString(length);
            _writer.WriteValue(value);
        }

        private bool? ReadLiteral(ref PacketReader reader)
        {
            int value = reader.ReadInt(1);
            return value switch
            {
                0x00 => null,
                0x01 => true,
                0x02 => false,
                _ => throw new FormatException($"Unexpected JSON literal value {value}")
            };
        }

        private int ReadJsonSize(ref PacketReader reader, bool small)
        {
            long result = small ? reader.ReadInt(2) : reader.ReadLong(4);

            if (result > int.MaxValue)
                throw new FormatException("JSON offset or length field is too big");

            return (int)result;
        }

        private int ReadDataLength(ref PacketReader reader)
        {
            int length = 0;
            for (int i = 0; i < 5; i++)
            {
                byte value = (byte)reader.ReadInt(1);
                length |= (value & 0x7F) << (7 * i);
                if ((value & 0x80) == 0)
                    return length;
            }
            throw new FormatException("Unexpected JSON data length");
        }
    }
}
