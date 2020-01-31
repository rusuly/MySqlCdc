using System.Buffers;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets
{
    /// <summary>
    /// ERR_Packet indicates that an error occured.
    /// <a href="https://mariadb.com/kb/en/library/err_packet/">See more</a>
    /// </summary>
    internal class ErrorPacket : IPacket
    {
        public int ErrorCode { get; }
        public string SqlState { get; }
        public string ErrorMessage { get; }

        public ErrorPacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);

            ErrorCode = reader.ReadInt(2);

            var message = reader.ReadStringToEndOfFile();
            if (message.StartsWith("#"))
            {
                SqlState = message.Substring(1, 5);
                ErrorMessage = message.Substring(6);
            }
            else
            {
                ErrorMessage = message;
            }
        }

        public override string ToString()
        {
            return $"ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}, SqlState:{SqlState}";
        }
    }
}
