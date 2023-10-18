using MySqlCdc.Events;

namespace MySqlCdc.Providers.MariaDb;

/// <summary>
/// MariaDB 10.0.2+ representation of Gtid.
/// </summary>
/// <remarks>
/// Creates a new <see cref="Gtid"/>.
/// </remarks>
public record Gtid(long DomainId, long ServerId, long Sequence) : IGtid
{
    /// <summary>
    /// Gets domain identifier in multi-master setup.
    /// </summary>
    public long DomainId { get; } = DomainId;

    /// <summary>
    /// Gets identifier of the server that generated the event.
    /// </summary>
    public long ServerId { get; } = ServerId;

    /// <summary>
    /// Gets sequence number of the event on the original server.
    /// </summary>
    public long Sequence { get; } = Sequence;

    /// <summary>
    /// Returns string representation of Gtid in MariaDB.
    /// </summary>
    public override string ToString() => $"{DomainId}-{ServerId}-{Sequence}";
}