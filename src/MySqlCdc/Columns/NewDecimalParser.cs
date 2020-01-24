using System.Buffers;
using System.Text;
using MySqlCdc.Protocol;

namespace MySqlCdc.Columns
{
    /// <summary>
    /// See <a href="https://github.com/noplay/python-mysql-replication/blob/511b42c8ac2c1682a6e2fd4d6691658245b57987/pymysqlreplication/row_event.py#L347">example</a>
    /// </summary>
    internal class NewDecimalParser : IColumnParser
    {
        private const int DigitsPerInt = 9;
        private static readonly int[] CompressedBytes = new int[] { 0, 1, 1, 2, 2, 3, 3, 4, 4, 4 };

        public object ParseColumn(ref PacketReader reader, int metadata)
        {
            int precision = metadata & 0xFF;
            int scale = metadata >> 8;
            int integral = precision - scale;

            int uncompressedIntegral = integral / DigitsPerInt;
            int uncompressedFractional = scale / DigitsPerInt;
            int compressedIntegral = integral - (uncompressedIntegral * DigitsPerInt);
            int compressedFractional = scale - (uncompressedFractional * DigitsPerInt);
            int length =
                (uncompressedIntegral << 2) + CompressedBytes[compressedIntegral] +
                (uncompressedFractional << 2) + CompressedBytes[compressedFractional];

            byte[] value = reader.ReadByteArraySlow(length);
            var result = new StringBuilder();

            bool negative = (value[0] & 0x80) == 0;
            value[0] ^= 0x80;

            if (negative)
            {
                result.Append("-");
                for (int i = 0; i < value.Length; i++)
                    value[i] ^= 0xFF;
            }

            var buffer = new PacketReader(new ReadOnlySequence<byte>(value));

            int size = CompressedBytes[compressedIntegral];
            if (size > 0)
            {
                result.Append(buffer.ReadBigEndianInt(size));
            }
            for (int i = 0; i < uncompressedIntegral; i++)
            {
                result.Append(buffer.ReadBigEndianInt(4).ToString("D9"));
            }
            result.Append(".");

            size = CompressedBytes[compressedFractional];
            for (int i = 0; i < uncompressedFractional; i++)
            {
                result.Append(buffer.ReadBigEndianInt(4).ToString("D9"));
            }
            if (size > 0)
            {
                result.Append(buffer.ReadBigEndianInt(size).ToString($"D{compressedFractional}"));
            }
            return result.ToString();
        }
    }
}
