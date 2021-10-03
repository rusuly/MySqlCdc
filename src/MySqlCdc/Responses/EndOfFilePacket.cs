using System;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets;

/// <summary>
/// EOF packet marks the end of a resultset and returns status and warnings.
/// <a href="https://mariadb.com/kb/en/library/eof_packet/">See more</a>
/// </summary>
internal class EndOfFilePacket : IPacket
{
    public int WarningCount { get; }
    public int ServerStatus { get; }

    public EndOfFilePacket(ReadOnlySpan<byte> span)
    {
        var reader = new PacketReader(span);

        WarningCount = reader.ReadUInt16LittleEndian();
        ServerStatus = reader.ReadUInt16LittleEndian();
    }
}