using MySqlCdc.Providers.MySql;

namespace MySqlCdc.Events
{
    /// <summary>
    /// Used to record the gtid_executed of previous binlog files.
    /// </summary>
    public class PreviousGtidsEvent : BinlogEvent
    {
        /// <summary>
        /// Gets GtidSet of previous files.
        /// </summary>
        public GtidSet GtidSet { get; }

        /// <summary>
        /// Creates a new <see cref="PreviousGtidsEvent"/>.
        /// </summary>
        public PreviousGtidsEvent(EventHeader header, GtidSet gtidSet) : base(header)
        {
            GtidSet = gtidSet;
        }
    }
}
