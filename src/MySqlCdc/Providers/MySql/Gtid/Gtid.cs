using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql
{
    /// <summary>
    /// MySQL 5.6+ representation of Gtid.
    /// </summary>
    public class Gtid : IGtid
    {
        /// <summary>
        /// Gets identifier of the original server that generated the event.
        /// </summary>
        public Uuid SourceId { get; }

        /// <summary>
        /// Gets sequence number of the event on the original server.
        /// </summary>
        public long TransactionId { get; }

        /// <summary>
        /// Creates a new <see cref="Gtid"/>.
        /// </summary>
        public Gtid(Uuid sourceId, long transactionId)
        {
            SourceId = sourceId;
            TransactionId = transactionId;
        }

        /// <summary>
        /// Returns string representation of Gtid in MySQL Server.
        /// </summary>
        public override string ToString() => $"{SourceId}:{TransactionId}";
    }
}
