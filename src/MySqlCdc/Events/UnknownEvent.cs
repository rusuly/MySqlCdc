namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents other binlog events.
    /// </summary>
    public class UnknownEvent : BinlogEvent
    {
        /// <summary>
        /// Creates a new <see cref="UnknownEvent"/>.
        /// </summary>
        public UnknownEvent(EventHeader header) : base(header)
        {
        }
    }
}
