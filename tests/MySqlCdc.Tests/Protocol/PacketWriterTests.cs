using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol;

public class PacketWriterTests
{
    private const string TestString = "Lorem ipsum dolor sit amet";

    [Fact]
    public void Test_WriteByte_ReturnsPacket()
    {
        var writer = new PacketWriter(31);
        writer.WriteByte(250);
        writer.WriteByte(110);

        var expected = new byte[]
        {
            2, 0, 0, 31,
            250, 110
        };
        Assert.Equal(expected, writer.CreatePacket());
    }

    [Fact]
    public void Test_WriteByteArray_ReturnsPacket()
    {
        var writer = new PacketWriter(32);
        writer.WriteByteArray(new byte[] { 250, 110, 10, 150 });

        var expected = new byte[]
        {
            4, 0, 0, 32,
            250, 110, 10, 150
        };
        Assert.Equal(expected, writer.CreatePacket());
    }

    [Fact]
    public void Test_WriteIntLittleEndian_ReturnsPacket()
    {
        var writer = new PacketWriter(32);

        writer.WriteIntLittleEndian(123, 1);
        writer.WriteIntLittleEndian(12345, 2);
        writer.WriteIntLittleEndian(1234567, 3);
        writer.WriteIntLittleEndian(2123456789, 4);

        var expected = new byte[]
        {
            10, 0, 0, 32,
            0x7B, 0x39, 0x30, 0x87, 0xD6, 0x12, 0x15, 0x61, 0x91, 0x7E
        };
        Assert.Equal(expected, writer.CreatePacket());
    }

    [Fact]
    public void Test_WriteLongLittleEndian_ReturnsPacket()
    {
        var writer = new PacketWriter(0);

        writer.WriteLongLittleEndian(12345678909, 5);
        writer.WriteLongLittleEndian(1234567890987, 6);
        writer.WriteLongLittleEndian(1234567890987654, 7);
        writer.WriteLongLittleEndian(1234567890987654321, 8);

        var expected = new byte[]
        {
            26, 0, 0, 0,
            0x3D, 0x1C, 0xDC, 0xDF, 0x02,
            0x2B, 0x08, 0xFB, 0x71, 0x1F, 0x01,
            0x86, 0xEA, 0x97, 0x3C, 0xD5, 0x62, 0x04,
            0xB1, 0x1C, 0x6C, 0xB1, 0xF4, 0x10, 0x22, 0x11
        };
        Assert.Equal(expected, writer.CreatePacket());
    }

    [Fact]
    public void Test_WriteString_ReturnsPacket()
    {
        var writer = new PacketWriter(3);
        writer.WriteString(TestString);

        var expected = new byte[]
        {
            26, 0, 0, 3,
            76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109, 32, 100, 111, 108, 111, 114, 32, 115, 105, 116, 32, 97, 109, 101, 116
        };
        Assert.Equal(expected, writer.CreatePacket());
    }

    [Fact]
    public void Test_WriteNullTerminatedString_ReturnsPacket()
    {
        var writer = new PacketWriter(3);
        writer.WriteNullTerminatedString(TestString);

        var expected = new byte[]
        {
            27, 0, 0, 3,
            76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109, 32, 100, 111, 108, 111, 114, 32, 115, 105, 116, 32, 97, 109, 101, 116, 0
        };
        Assert.Equal(expected, writer.CreatePacket());
    }
}