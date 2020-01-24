using System.Collections;
using System.Collections.Generic;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    public class DeleteRowsEventParser : RowEventParser, IEventParser
    {
        public DeleteRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(tableMapCache, rowsEventVersion)
        {
        }

        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var shared = ParseHeader(ref reader);

            var columnsPresent = reader.ReadBitmap(shared.columnsNumber);
            var rows = ParseDeleteRows(ref reader, shared.tableId, columnsPresent);

            return new DeleteRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsPresent, rows);
        }

        private IReadOnlyList<ColumnData> ParseDeleteRows(ref PacketReader reader, long tableId, BitArray columnsPresent)
        {
            var rows = new List<ColumnData>();
            while (!reader.IsEmpty())
            {
                rows.Add(ParseRow(ref reader, tableId, columnsPresent));
            }
            return rows;
        }
    }
}
