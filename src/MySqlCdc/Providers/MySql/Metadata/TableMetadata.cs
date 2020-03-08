using System;
using System.Buffers;
using System.Collections.Generic;
using MySqlCdc.Constants;
using MySqlCdc.Protocol;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// Contains metadata for table columns.
    /// <a href="https://dev.mysql.com/doc/dev/mysql-server/latest/classbinary__log_1_1Table__map__event.html">See more</a>
    /// </summary>
    public class TableMetadata
    {
        /// <summary>
        /// Gets signedness of numeric colums.
        /// </summary>
        public bool[] Signedness { get; }

        /// <summary>
        /// Gets charsets of character columns.
        /// </summary>
        public DefaultCharset DefaultCharset { get; }

        /// <summary>
        /// Gets charsets of character columns.
        /// </summary>
        public IReadOnlyList<int> ColumnCharsets { get; }

        /// <summary>
        /// Gets column names.
        /// </summary>
        public IReadOnlyList<string> ColumnNames { get; }

        /// <summary>
        /// Gets string values of SET columns.
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> SetStringValues { get; }

        /// <summary>
        /// Gets string values of ENUM columns
        /// </summary>
        public IReadOnlyList<IReadOnlyList<string>> EnumStringValues { get; }

        /// <summary>
        /// Gets real types of geometry columns.
        /// </summary>
        public IReadOnlyList<int> GeometryTypes { get; }

        /// <summary>
        /// Gets primary keys without prefixes.
        /// </summary>
        public IReadOnlyList<int> SimplePrimaryKeys { get; }

        /// <summary>
        /// Gets primary keys with prefixes.
        /// </summary>
        public IReadOnlyDictionary<int, int> PrimaryKeysWithPrefix { get; }

        /// <summary>
        /// Gets charsets of ENUM and SET columns.
        /// </summary>
        public DefaultCharset EnumAndSetDefaultCharset { get; }

        /// <summary>
        /// Gets charsets of ENUM and SET columns.
        /// </summary>
        public IReadOnlyList<int> EnumAndSetColumnCharsets { get; }

        /// <summary>
        /// Creates a new <see cref="TableMetadata"/>.
        /// </summary>
        public TableMetadata(ref PacketReader reader, byte[] columnTypes)
        {
            while (!reader.IsEmpty())
            {
                var metadataType = (MetadataType)reader.ReadByte();
                int metadataLength = (int)reader.ReadLengthEncodedNumber();

                var metadata = reader.ReadByteArraySlow(metadataLength);

                using var memoryOwner = new MemoryOwner(new ReadOnlySequence<byte>(metadata));
                var buffer = new PacketReader(memoryOwner.Memory.Span);

                switch (metadataType)
                {
                    case MetadataType.SIGNEDNESS:
                        Signedness = ReadSignednessBitmap(ref buffer, GetNumericColumnCount(columnTypes));
                        break;
                    case MetadataType.DEFAULT_CHARSET:
                        DefaultCharset = ParseDefaultCharset(ref buffer);
                        break;
                    case MetadataType.COLUMN_CHARSET:
                        ColumnCharsets = ParseIntArray(ref buffer);
                        break;
                    case MetadataType.COLUMN_NAME:
                        ColumnNames = ParseStringArray(ref buffer);
                        break;
                    case MetadataType.SET_STR_VALUE:
                        SetStringValues = ParseTypeValues(ref buffer);
                        break;
                    case MetadataType.ENUM_STR_VALUE:
                        EnumStringValues = ParseTypeValues(ref buffer);
                        break;
                    case MetadataType.GEOMETRY_TYPE:
                        GeometryTypes = ParseIntArray(ref buffer);
                        break;
                    case MetadataType.SIMPLE_PRIMARY_KEY:
                        SimplePrimaryKeys = ParseIntArray(ref buffer);
                        break;
                    case MetadataType.PRIMARY_KEY_WITH_PREFIX:
                        PrimaryKeysWithPrefix = ParseIntMap(ref buffer);
                        break;
                    case MetadataType.ENUM_AND_SET_DEFAULT_CHARSET:
                        EnumAndSetDefaultCharset = ParseDefaultCharset(ref buffer);
                        break;
                    case MetadataType.ENUM_AND_SET_COLUMN_CHARSET:
                        EnumAndSetColumnCharsets = ParseIntArray(ref buffer);
                        break;
                    default:
                        throw new InvalidOperationException($"Table metadata type {metadataType} is not supported");
                }
            }
        }

        private DefaultCharset ParseDefaultCharset(ref PacketReader reader)
        {
            int defaultCharsetCollation = reader.ReadLengthEncodedNumber();
            var charsetCollations = ParseIntMap(ref reader);
            return new DefaultCharset(defaultCharsetCollation, charsetCollations);
        }

        private IReadOnlyDictionary<int, int> ParseIntMap(ref PacketReader reader)
        {
            var result = new Dictionary<int, int>();
            while (!reader.IsEmpty())
            {
                int key = reader.ReadLengthEncodedNumber();
                int value = reader.ReadLengthEncodedNumber();
                result[key] = value;
            }
            return result;
        }

        private IReadOnlyList<int> ParseIntArray(ref PacketReader reader)
        {
            var result = new List<int>();
            while (!reader.IsEmpty())
            {
                int value = reader.ReadLengthEncodedNumber();
                result.Add(value);
            }
            return result;
        }

        private IReadOnlyList<string> ParseStringArray(ref PacketReader reader)
        {
            var result = new List<string>();
            while (!reader.IsEmpty())
            {
                string value = reader.ReadLengthEncodedString();
                result.Add(value);
            }
            return result;
        }

        private IReadOnlyList<IReadOnlyList<string>> ParseTypeValues(ref PacketReader reader)
        {
            var result = new List<IReadOnlyList<string>>();
            while (!reader.IsEmpty())
            {
                int length = reader.ReadLengthEncodedNumber();
                var typeValues = new string[length];
                for (int i = 0; i < length; i++)
                {
                    typeValues[i] = reader.ReadLengthEncodedString();
                }
                result.Add(typeValues);
            }
            return result;
        }

        private bool[] ReadSignednessBitmap(ref PacketReader reader, int bitsNumber)
        {
            var result = new bool[bitsNumber];
            var bytesNumber = (bitsNumber + 7) / 8;
            for (int i = 0; i < bytesNumber; i++)
            {
                byte value = reader.ReadByte();
                for (int y = 0; y < 8; y++)
                {
                    int index = (i << 3) + y;
                    if (index == bitsNumber)
                        break;

                    // The difference from ReadBitmap is that bits are reverted
                    result[index] = (value & (1 << (7 - y))) > 0;
                }
            }
            return result;
        }

        private int GetNumericColumnCount(byte[] columnTypes)
        {
            int count = 0;
            for (int i = 0; i < columnTypes.Length; i++)
            {
                switch ((ColumnType)columnTypes[i])
                {
                    case ColumnType.TINY:
                    case ColumnType.SHORT:
                    case ColumnType.INT24:
                    case ColumnType.LONG:
                    case ColumnType.LONGLONG:
                    case ColumnType.FLOAT:
                    case ColumnType.DOUBLE:
                    case ColumnType.NEWDECIMAL:
                        count++;
                        break;
                    default:
                        break;
                }
            }
            return count;
        }
    }
}
