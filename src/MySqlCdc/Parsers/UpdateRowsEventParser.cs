using System.Collections;
using System.Collections.Generic;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Parses <see cref="UpdateRowsEvent"/> events.
    /// Supports all versions of MariaDB and MySQL 5.5+ (V1 and V2 row events).
    /// </summary>
    public class UpdateRowsEventParser : RowEventParser, IEventParser
    {
        /// <summary>
        /// Creates a new <see cref="UpdateRowsEventParser"/>.
        /// </summary>
        public UpdateRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(tableMapCache, rowsEventVersion)
        {
        }

        /// <summary>
        /// Parses <see cref="UpdateRowsEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var shared = ParseHeader(ref reader);

            var columnsBeforeUpdate = reader.ReadBitmap(shared.columnsNumber);
            var columnsAfterUpdate = reader.ReadBitmap(shared.columnsNumber);

            var rows = ParseUpdateRows(ref reader, shared.tableId, columnsBeforeUpdate, columnsAfterUpdate);
            return new UpdateRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsBeforeUpdate, columnsAfterUpdate, rows);
        }

        private IReadOnlyList<UpdateColumnData> ParseUpdateRows(
            ref PacketReader reader,
            long tableId,
            BitArray columnsBeforeUpdate,
            BitArray columnsAfterUpdate)
        {
            var rows = new List<UpdateColumnData>();
            while (!reader.IsEmpty())
            {
                var rowBeforeUpdate = ParseRow(ref reader, tableId, columnsBeforeUpdate);
                var rowAfterUpdate = ParseRow(ref reader, tableId, columnsAfterUpdate);

                rows.Add(new UpdateColumnData(rowBeforeUpdate, rowAfterUpdate));
            }
            return rows;
        }
    }
}
