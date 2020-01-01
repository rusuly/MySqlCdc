using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class WriteRowsEventParser : RowEventParser
    {
        public WriteRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(tableMapCache, rowsEventVersion)
        {
        }

        public override IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            var shared = ParseHeader(reader);

            var columnsPresent = reader.ReadBitmap(shared.columnsNumber);
            var rows = ParseWriteRows(reader, shared.tableId, columnsPresent);

            return new WriteRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsPresent, rows);
        }

        private IReadOnlyList<ColumnData> ParseWriteRows(PacketReader reader, long tableId, BitArray columnsPresent)
        {
            var rows = new List<ColumnData>();
            while (!reader.IsEmpty())
            {
                rows.Add(ParseRow(reader, tableId, columnsPresent));
            }
            return rows;
        }
    }
}
