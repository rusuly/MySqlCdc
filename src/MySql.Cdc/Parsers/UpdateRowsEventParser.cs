using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using MySql.Cdc.Events;
using MySql.Cdc.Protocol;

namespace MySql.Cdc.Parsers
{
    public class UpdateRowsEventParser : RowEventParser
    {
        public UpdateRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(tableMapCache, rowsEventVersion)
        {
        }

        public override IBinlogEvent ParseEvent(EventHeader header, ReadOnlySequence<byte> buffer)
        {
            var reader = new PacketReader(buffer);
            var shared = ParseHeader(reader);

            var columnsBeforeUpdate = reader.ReadBitmap(shared.columnsNumber);
            var columnsAfterUpdate = reader.ReadBitmap(shared.columnsNumber);

            var rows = ParseUpdateRows(reader, shared.tableId, columnsBeforeUpdate, columnsAfterUpdate);
            return new UpdateRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsBeforeUpdate, columnsAfterUpdate, rows);
        }

        private IReadOnlyList<UpdateColumnData> ParseUpdateRows(
            PacketReader reader,
            long tableId,
            BitArray columnsBeforeUpdate,
            BitArray columnsAfterUpdate)
        {
            var rows = new List<UpdateColumnData>();
            while (!reader.IsEmpty())
            {
                var rowBeforeUpdate = ParseRow(reader, tableId, columnsBeforeUpdate);
                var rowAfterUpdate = ParseRow(reader, tableId, columnsAfterUpdate);

                rows.Add(new UpdateColumnData(rowBeforeUpdate, rowAfterUpdate));
            }
            return rows;
        }
    }
}
