using MySqlCdc.Constants;
using MySqlCdc.Protocol;
using Xunit;

namespace MySqlCdc.Tests.Protocol;

public class PacketReaderTests
{
    private static byte[] NumericPayload = new byte[]
    {
        250,  110,   10, 150,
        23,    0,   13, 255,
        3,   50,   80, 130,
        220,   75,  250,  78,
        45,   99
    };

    [Fact]
    public void Test_ReadByte_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(NumericPayload[0], reader.ReadByte());
        Assert.Equal(1, reader.Consumed);

        Assert.Equal(NumericPayload[1], reader.ReadByte());
        Assert.Equal(2, reader.Consumed);
    }

    [Fact]
    public void Test_ReadUInt16LittleEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0x6EFA, reader.ReadUInt16LittleEndian());
        Assert.Equal(2, reader.Consumed);

        Assert.Equal(0x960A, reader.ReadUInt16LittleEndian());
        Assert.Equal(4, reader.Consumed);
    }

    [Fact]
    public void Test_ReadUInt16BigEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0xFA6E, reader.ReadUInt16BigEndian());
        Assert.Equal(2, reader.Consumed);

        Assert.Equal(0x0A96, reader.ReadUInt16BigEndian());
        Assert.Equal(4, reader.Consumed);
    }

    [Fact]
    public void Test_ReadUInt32LittleEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0x960A6EFA, reader.ReadUInt32LittleEndian());
        Assert.Equal(4, reader.Consumed);

        Assert.Equal(0xFF0D0017, reader.ReadUInt32LittleEndian());
        Assert.Equal(8, reader.Consumed);
    }

    [Fact]
    public void Test_ReadUInt32BigEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0xFA6E0A96, reader.ReadUInt32BigEndian());
        Assert.Equal(4, reader.Consumed);

        Assert.Equal((uint)0x17000DFF, reader.ReadUInt32BigEndian());
        Assert.Equal(8, reader.Consumed);
    }

    [Fact]
    public void Test_ReadInt64LittleEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0xFF0D0017960A6EFA, (ulong)reader.ReadInt64LittleEndian());
        Assert.Equal(8, reader.Consumed);

        Assert.Equal((ulong)0x4EFA4BDC82503203, (ulong)reader.ReadInt64LittleEndian());
        Assert.Equal(16, reader.Consumed);
    }

    [Fact]
    public void Test_ReadIntLittleEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0xFA, reader.ReadIntLittleEndian(1));
        Assert.Equal(1, reader.Consumed);

        Assert.Equal(0x0A6E, reader.ReadIntLittleEndian(2));
        Assert.Equal(3, reader.Consumed);

        Assert.Equal(0x001796, reader.ReadIntLittleEndian(3));
        Assert.Equal(6, reader.Consumed);
    }

    [Fact]
    public void Test_ReadLongLittleEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0x17960A6EFA, reader.ReadLongLittleEndian(5));
        Assert.Equal(5, reader.Consumed);

        Assert.Equal(0x503203FF0D00, reader.ReadLongLittleEndian(6));
        Assert.Equal(11, reader.Consumed);

        Assert.Equal(0x632D4EFA4BDC82, reader.ReadLongLittleEndian(7));
        Assert.Equal(18, reader.Consumed);
    }

    [Fact]
    public void Test_ReadIntBigEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0xFA, reader.ReadIntBigEndian(1));
        Assert.Equal(1, reader.Consumed);

        Assert.Equal(0x6E0A, reader.ReadIntBigEndian(2));
        Assert.Equal(3, reader.Consumed);

        Assert.Equal(0x961700, reader.ReadIntBigEndian(3));
        Assert.Equal(6, reader.Consumed);
    }

    [Fact]
    public void Test_ReadLongBigEndian_AdvancesOffset()
    {
        var reader = new PacketReader(NumericPayload);

        Assert.Equal(0xFA6E0A9617, reader.ReadLongBigEndian(5));
        Assert.Equal(5, reader.Consumed);

        Assert.Equal(0x000DFF033250, reader.ReadLongBigEndian(6));
        Assert.Equal(11, reader.Consumed);

        Assert.Equal(0x82DC4BFA4E2D63, reader.ReadLongBigEndian(7));
        Assert.Equal(18, reader.Consumed);
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFA_ReturnsTheByte()
    {
        var payload = new byte[] { 0xFA };
        var reader = ReadLengthEncodedNumber(payload);

        Assert.Equal(0xFA, reader.number);
        Assert.Equal(1, reader.consumed);
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFB_ThrowsFormatException()
    {
        var payload = new byte[] { 0xFB };
        Assert.Throws<FormatException>(() => ReadLengthEncodedNumber(payload));
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFC_ReturnsInt16()
    {
        var payload = new byte[] { 0xFC, 0xFA, 0x6E };
        var reader = ReadLengthEncodedNumber(payload);

        Assert.Equal(0x6EFA, reader.number);
        Assert.Equal(3, reader.consumed);
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFD_ReturnsInt24()
    {
        var payload = new byte[] { 0xFD, 0x0A, 0xFA, 0x6E };
        var reader = ReadLengthEncodedNumber(payload);

        Assert.Equal(0x6EFA0A, reader.number);
        Assert.Equal(4, reader.consumed);
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFE_ReturnsInt64()
    {
        var payload = new byte[] { 0xFE, 0x0A, 0xFA, 0x6E, 0x40, 0, 0, 0, 0 };
        var reader = ReadLengthEncodedNumber(payload);

        Assert.Equal(0x406EFA0A, reader.number);
        Assert.Equal(9, reader.consumed);
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFE_ThrowsOverflowException()
    {
        var payload = new byte[] { 0xFE, 0x0A, 0xFA, 0x6E, 0x90, 0, 0, 0, 0 };
        Assert.Throws<OverflowException>(() => ReadLengthEncodedNumber(payload));
    }

    [Fact]
    public void Test_ReadLengthEncoded_0xFF_ThrowsFormatException()
    {
        var payload = new byte[] { 0xFF };
        Assert.Throws<FormatException>(() => ReadLengthEncodedNumber(payload));
    }

    [Fact]
    public void Test_ReadString_ReturnsEmptyString()
    {
        var payload = new byte[] { 123 };
        var reader = new PacketReader(payload);

        Assert.Equal(string.Empty, reader.ReadString(0));
        Assert.Equal(0, reader.Consumed);
    }

    [Fact]
    public void Test_ReadString_ReturnsFixedString()
    {
        var payload = new byte[] { 76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109 };
        var reader = new PacketReader(payload);

        Assert.Equal("Lorem", reader.ReadString(5));
        Assert.Equal(5, reader.Consumed);
    }

    [Fact]
    public void Test_ReadStringToEndOfFile_ReturnsString()
    {
        var payload = new byte[] { 76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109 };
        var reader = new PacketReader(payload);

        Assert.Equal("Lorem ipsum", reader.ReadStringToEndOfFile());
        Assert.Equal(11, reader.Consumed);
    }

    [Fact]
    public void Test_ReadNullTerminatedString_ReturnsString()
    {
        var payload = new byte[] { 76, 111, 114, 101, 109, PacketConstants.NullTerminator, 105, 112, 115, 117, 109 };
        var reader = new PacketReader(payload);

        Assert.Equal("Lorem", reader.ReadNullTerminatedString());
        Assert.Equal("Lorem".Length + 1, reader.Consumed);
    }

    [Fact]
    public void Test_ReadLengthEncodedString_ReturnsString()
    {
        const byte length = 5;
        var payload = new byte[] { length, 76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109 };
        var reader = new PacketReader(payload);

        Assert.Equal("Lorem", reader.ReadLengthEncodedString());
        Assert.Equal(length + 1, reader.Consumed);
    }

    [Fact]
    public void Test_ReadByteArraySlow_ReturnsArray()
    {
        var payload = new byte[] { 76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109 };
        var reader = new PacketReader(payload);

        Assert.Equal(new byte[] { 76, 111, 114, 101, 109 }, reader.ReadByteArraySlow(5));
        Assert.Equal(5, reader.Consumed);
    }

    [Fact]
    public void Test_ReadBitmapLittleEndian_ReturnsBitmap()
    {
        var payload = new byte[] { 0xB4, 0x78 };
        var reader = new PacketReader(payload);

        var expected = new byte[]
        {
            0, 0, 1, 0, 1, 1, 0, 1,
            0, 0, 0, 1, 1, 1
        }.Select(x => x > 0).ToArray();

        Assert.Equal(expected, reader.ReadBitmapLittleEndian(14));
        Assert.Equal(2, reader.Consumed);
    }

    [Fact]
    public void Test_ReadBitmapBigEndian_ReturnsBitmap()
    {
        var payload = new byte[] { 0xB4, 0x78 };
        var reader = new PacketReader(payload);

        var expected = new byte[]
        {
            0, 0, 0, 1, 1, 1, 1, 0,
            0, 0, 1, 0, 1, 1
        }.Select(x => x > 0).ToArray();

        Assert.Equal(expected, reader.ReadBitmapBigEndian(14));
        Assert.Equal(2, reader.Consumed);
    }

    [Fact]
    public void Test_Advance_ReturnsIsEmpty()
    {
        var payload = new byte[] { 76, 111, 114, 101, 109, 32, 105, 112, 115, 117, 109 };
        var reader = new PacketReader(payload);

        reader.Advance(5);
        Assert.False(reader.IsEmpty());
        Assert.Equal(5, reader.Consumed);

        reader.Advance(6);
        Assert.True(reader.IsEmpty());
        Assert.Equal(11, reader.Consumed);
    }

    private (int number, int consumed) ReadLengthEncodedNumber(byte[] payload)
    {
        var reader = new PacketReader(payload);
        int number = reader.ReadLengthEncodedNumber();
        int consumed = reader.Consumed;
        return (number, consumed);
    }
}