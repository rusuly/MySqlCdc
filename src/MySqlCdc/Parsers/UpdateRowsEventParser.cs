using System;
using System.Collections.Generic;
using MySqlCdc.Constants;
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
        private Dictionary<long, TableMapEvent> _tableMapCache { get; }

        /// <summary>
        /// Creates a new <see cref="UpdateRowsEventParser"/>.
        /// </summary>
        public UpdateRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(rowsEventVersion)
        {
            _tableMapCache = tableMapCache;
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
            bool[] columnsBeforeUpdate,
            bool[] columnsAfterUpdate)
        {
            var cellsIncludedBeforeUpdate = GetBitsNumber(columnsBeforeUpdate);
            var cellsIncludedAfterUpdate = GetBitsNumber(columnsAfterUpdate);
            if (!_tableMapCache.TryGetValue(tableId, out var tableMap))
                throw new InvalidOperationException(EventConstants.TableMapNotFound);

            var rows = new List<UpdateColumnData>();
            while (!reader.IsEmpty())
            {
                var rowBeforeUpdate = ParseRow(ref reader, tableMap, columnsBeforeUpdate, cellsIncludedBeforeUpdate);
                var rowAfterUpdate = ParseRow(ref reader, tableMap, columnsAfterUpdate, cellsIncludedAfterUpdate);

                rows.Add(new UpdateColumnData(rowBeforeUpdate, rowAfterUpdate));
            }
            return rows;
        }
    }
}
