using System.Text;
using MySqlCdc.Constants;

namespace MySqlCdc.Protocol;

/// <summary>
/// Creates packet for a command from sequence of writes.
/// </summary>
internal class PacketWriter : IDisposable
{
    private readonly MemoryStream _stream;

    public PacketWriter()
    {
        _stream = new MemoryStream();
    }

    /// <summary>
    /// Writes byte to the stream.
    /// </summary>
    public void WriteByte(byte value)
    {
        _stream.WriteByte(value);
    }

    /// <summary>
    /// Writes byte array to the stream.
    /// </summary>
    public void WriteByteArray(byte[] array)
    {
        _stream.Write(array, 0, array.Length);
    }

    /// <summary>
    /// Writes int in little-endian format.
    /// </summary>
    public void WriteIntLittleEndian(int number, int length)
    {
        for (int i = 0; i < length; i++)
        {
            byte value = (byte)(0xFF & ((uint)number >> (i << 3)));
            _stream.WriteByte(value);
        }
    }

    /// <summary>
    /// Writes long in little-endian format.
    /// </summary>
    public void WriteLongLittleEndian(long number, int length)
    {
        for (int i = 0; i < length; i++)
        {
            byte value = (byte)(0xFF & ((ulong)number >> (i << 3)));
            _stream.WriteByte(value);
        }
    }

    /// <summary>
    /// Writes end-of-file length string.
    /// </summary>
    public void WriteString(string value)
    {
        WriteByteArray(Encoding.UTF8.GetBytes(value));
    }

    /// <summary>
    /// Writes null-terminated string.
    /// </summary>
    public void WriteNullTerminatedString(string value)
    {
        WriteByteArray(Encoding.UTF8.GetBytes(value));
        _stream.WriteByte(PacketConstants.NullTerminator);
    }

    public byte[] CreatePacket()
    {
        return _stream.ToArray();
    }

    public void Dispose()
    {
        _stream.Dispose();
    }
}