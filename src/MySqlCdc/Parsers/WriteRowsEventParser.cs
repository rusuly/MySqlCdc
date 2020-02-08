using System;
using System.Collections.Generic;
using MySqlCdc.Constants;
using MySqlCdc.Events;
using MySqlCdc.Protocol;

namespace MySqlCdc.Parsers
{
    /// <summary>
    /// Parses <see cref="WriteRowsEvent"/> events.
    /// Supports all versions of MariaDB and MySQL 5.5+ (V1 and V2 row events).
    /// </summary>
    public class WriteRowsEventParser : RowEventParser, IEventParser
    {
        private Dictionary<long, TableMapEvent> _tableMapCache { get; }

        /// <summary>
        /// Creates a new <see cref="WriteRowsEventParser"/>.
        /// </summary>
        public WriteRowsEventParser(Dictionary<long, TableMapEvent> tableMapCache, int rowsEventVersion)
            : base(rowsEventVersion)
        {
            _tableMapCache = tableMapCache;
        }

        /// <summary>
        /// Parses <see cref="WriteRowsEvent"/> from the buffer.
        /// </summary>
        public IBinlogEvent ParseEvent(EventHeader header, ref PacketReader reader)
        {
            var shared = ParseHeader(ref reader);

            var columnsPresent = reader.ReadBitmap(shared.columnsNumber);
            var rows = ParseWriteRows(ref reader, shared.tableId, columnsPresent);

            return new WriteRowsEvent(header, shared.tableId, shared.flags, shared.columnsNumber, columnsPresent, rows);
        }

        private IReadOnlyList<ColumnData> ParseWriteRows(ref PacketReader reader, long tableId, bool[] columnsPresent)
        {
            var cellsIncluded = GetBitsNumber(columnsPresent);
            if (!_tableMapCache.TryGetValue(tableId, out var tableMap))
                throw new InvalidOperationException(EventConstants.TableMapNotFound);

            var rows = new List<ColumnData>();
            while (!reader.IsEmpty())
            {
                rows.Add(ParseRow(ref reader, tableMap, columnsPresent, cellsIncluded));
            }
            return rows;
        }
    }
}
