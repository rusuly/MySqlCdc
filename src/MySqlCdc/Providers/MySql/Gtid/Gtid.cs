using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql;

/// <summary>
/// MySQL 5.6+ representation of Gtid.
/// </summary>
/// <remarks>
/// Creates a new <see cref="Gtid"/>.
/// </remarks>
public record Gtid(Uuid SourceId, long TransactionId) : IGtid
{
    /// <summary>
    /// Gets identifier of the original server that generated the event.
    /// </summary>
    public Uuid SourceId { get; } = SourceId;

    /// <summary>
    /// Gets sequence number of the event on the original server.
    /// </summary>
    public long TransactionId { get; } = TransactionId;

    /// <summary>
    /// Returns string representation of Gtid in MySQL Server.
    /// </summary>
    public override string ToString() => $"{SourceId}:{TransactionId}";
}