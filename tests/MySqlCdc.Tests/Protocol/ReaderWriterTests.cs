using System;
using System.Buffers;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol
{
    public class ReaderWriterTests : IPacket
    {
        [Fact]
        public void Test_WrittenTypes_ReadCorrectly()
        {
            const int int8 = 123;
            const int int16 = 12345;
            const int int24 = 1234567;
            const int int32 = 2123456789;
            const string string1 = "Hello world!!!";
            const string string2 = "Lorem ipsum dolor sit amet";
            const string fixedString = "Aenean commodo ligula eget dolor";
            const string fixedString2 = "Excepteur sint occaecat cupidatat non proident";

            var writer = new PacketWriter(0);

            writer.WriteInt(int8, 1);
            writer.WriteInt(-int8, 1);
            writer.WriteInt(int16, 2);
            writer.WriteInt(-int16, 2);
            writer.WriteInt(int24, 3);
            writer.WriteInt(-int24, 3);
            writer.WriteInt(int32, 4);
            writer.WriteInt(-int32, 4);

            writer.WriteNullTerminatedString(string1);
            writer.WriteInt(string2.Length, 1);
            writer.WriteString(string2);
            writer.WriteString(fixedString);
            writer.WriteString(fixedString2);

            var reader = new PacketReader(new ReadOnlySpan<byte>(writer.CreatePacket()));

            //Read packet size and sequence number
            reader.ReadIntLittleEndian(3);
            reader.ReadIntLittleEndian(1);

            Assert.Equal(int8, reader.ReadIntLittleEndian(1));
            Assert.Equal(-int8, (reader.ReadIntLittleEndian(1) << 24) >> 24);
            Assert.Equal(int16, reader.ReadIntLittleEndian(2));
            Assert.Equal(-int16, (reader.ReadIntLittleEndian(2) << 16) >> 16);
            Assert.Equal(int24, reader.ReadIntLittleEndian(3));
            Assert.Equal(-int24, (reader.ReadIntLittleEndian(3) << 8) >> 8);
            Assert.Equal(int32, reader.ReadIntLittleEndian(4));
            Assert.Equal(-int32, reader.ReadIntLittleEndian(4));

            Assert.Equal(string1, reader.ReadNullTerminatedString());
            Assert.Equal(string2, reader.ReadLengthEncodedString());
            Assert.Equal(fixedString, reader.ReadString(fixedString.Length));
            Assert.Equal(fixedString2, reader.ReadStringToEndOfFile());
            Assert.True(reader.IsEmpty());
        }
    }
}
