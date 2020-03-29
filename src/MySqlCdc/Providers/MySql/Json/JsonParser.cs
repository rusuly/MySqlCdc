using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json;
using MySqlCdc.Columns;
using MySqlCdc.Protocol;

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
        private readonly static ColumnParser ColumnParser = new ColumnParser();

        /// <summary>
        /// Parses MySQL binary format json to a string.
        /// </summary>
        public static string Parse(byte[] data)
        {
            using var stream = new MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                JsonParser.Parse(data, new JsonWriter(writer));
            }
            return Encoding.UTF8.GetString(stream.ToArray());
        }

        /// <summary>
        /// Parses MySQL binary format using the provided writer.
        /// </summary>
        public static void Parse(byte[] data, IJsonWriter writer)
        {
            var parser = new JsonParser(writer);

            using var memoryOwner = new MemoryOwner(new ReadOnlySequence<byte>(data));
            var reader = new PacketReader(memoryOwner.Memory.Span);

            var valueType = parser.ReadValueType(ref reader);
            parser.ParseNode(ref reader, valueType);
        }

        private readonly IJsonWriter _writer;

        private JsonParser(IJsonWriter writer)
        {
            _writer = writer;
        }

        private ValueType ReadValueType(ref PacketReader reader) => (ValueType)reader.ReadByte();

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
                    _writer.WriteValue((Int16)reader.ReadUInt16LittleEndian());
                    break;
                case ValueType.UINT16:
                    _writer.WriteValue((UInt16)reader.ReadUInt16LittleEndian());
                    break;
                case ValueType.INT32:
                    _writer.WriteValue((Int32)reader.ReadUInt32LittleEndian());
                    break;
                case ValueType.UINT32:
                    _writer.WriteValue((UInt32)reader.ReadUInt32LittleEndian());
                    break;
                case ValueType.INT64:
                    _writer.WriteValue((Int64)reader.ReadInt64LittleEndian());
                    break;
                case ValueType.UINT64:
                    _writer.WriteValue((UInt64)reader.ReadInt64LittleEndian());
                    break;
                case ValueType.DOUBLE:
                    _writer.WriteValue((double)ColumnParser.ParseDouble(ref reader, 0));
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
                reader.Advance(holeSize);
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
                    keyLength[i] = reader.ReadUInt16LittleEndian();
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
                    reader.Advance(valueSize - 1);
                }
                else if (type == ValueType.INT16)
                {
                    entries[i] = ValueEntry.FromInlined(type, (Int16)reader.ReadUInt16LittleEndian());
                    reader.Advance(valueSize - 2);
                }
                else if (type == ValueType.UINT16)
                {
                    entries[i] = ValueEntry.FromInlined(type, (UInt16)reader.ReadUInt16LittleEndian());
                    reader.Advance(valueSize - 2);
                }
                else if (type == ValueType.INT32 && !small)
                {
                    entries[i] = ValueEntry.FromInlined(type, (Int32)reader.ReadUInt32LittleEndian());
                }
                else if (type == ValueType.UINT32 && !small)
                {
                    entries[i] = ValueEntry.FromInlined(type, (UInt32)reader.ReadUInt32LittleEndian());
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

                else if (entry.Type == ValueType.INT16)
                    _writer.WriteValue((Int16)entry.Value);
                else if (entry.Type == ValueType.UINT16)
                    _writer.WriteValue((UInt16)entry.Value);
                else if (entry.Type == ValueType.INT32)
                    _writer.WriteValue((Int32)entry.Value);
                else if (entry.Type == ValueType.UINT32)
                    _writer.WriteValue((UInt32)entry.Value);
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
            byte value = reader.ReadByte();
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
            long result = small ? reader.ReadUInt16LittleEndian() : reader.ReadUInt32LittleEndian();

            if (result > int.MaxValue)
                throw new FormatException("JSON offset or length field is too big");

            return (int)result;
        }

        private int ReadDataLength(ref PacketReader reader)
        {
            int length = 0;
            for (int i = 0; i < 5; i++)
            {
                byte value = reader.ReadByte();
                length |= (value & 0x7F) << (7 * i);
                if ((value & 0x80) == 0)
                    return length;
            }
            throw new FormatException("Unexpected JSON data length");
        }
    }
}
