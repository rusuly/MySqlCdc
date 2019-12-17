using System.Buffers;
using System.Collections.Generic;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Packets
{
    /// <summary>
    /// Returned in response to a QueryCommand.
    /// <see cref="https://mariadb.com/kb/en/library/resultset/"/>
    /// </summary>
    public class ResultSetRowPacket : IPacket
    {
        public string[] Cells { get; private set; }

        public ResultSetRowPacket(ReadOnlySequence<byte> sequence)
        {
            var reader = new PacketReader(sequence);

            var values = new List<string>();
            while (!reader.IsEmpty())
            {
                values.Add(reader.ReadLengthEncodedString());
            }
            Cells = values.ToArray();
        }
    }
}
