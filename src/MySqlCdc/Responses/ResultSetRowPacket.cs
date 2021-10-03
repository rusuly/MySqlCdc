using System;
using System.Collections.Generic;
using MySqlCdc.Protocol;

namespace MySqlCdc.Packets;

/// <summary>
/// Returned in response to a QueryCommand.
/// <a href="https://mariadb.com/kb/en/library/resultset/">See more</a>
/// </summary>
internal class ResultSetRowPacket : IPacket
{
    public IReadOnlyList<string> Cells { get; }

    public ResultSetRowPacket(ReadOnlySpan<byte> span)
    {
        var reader = new PacketReader(span);

        var values = new List<string>();
        while (!reader.IsEmpty())
        {
            values.Add(reader.ReadLengthEncodedString());
        }
        Cells = values;
    }
}