namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents a binlog event.
    /// </summary>
    public abstract class BinlogEvent : IBinlogEvent
    {
        /// <summary>
        /// Gets the event header
        /// </summary>
        public EventHeader Header { get; }

        /// <summary>
        /// Creates a new <see cref="BinlogEvent"/>.
        /// </summary>
        protected BinlogEvent(EventHeader header)
        {
            Header = header;
        }
    }
}
