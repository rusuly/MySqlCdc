using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class DeleteRowsEventParser : RowEventParser
    {
        public DeleteRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(tableMapCache, rowsEventVersion)
        {
        }

        public override IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            var shared = ParseHeader(reader);

            var columnsPresent = reader.ReadBitmap(shared.columnsNumber);
            var rows = ParseDeleteRows(reader, shared.tableId, columnsPresent);

            return new DeleteRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsPresent, rows);
        }

        private IReadOnlyList<ColumnData> ParseDeleteRows(PacketReader reader, long tableId, BitArray columnsPresent)
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
