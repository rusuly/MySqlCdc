using MySqlCdc.Events;

namespace MySqlCdc.Providers.MariaDb;

/// <summary>
/// MariaDB 10.0.2+ representation of Gtid.
/// </summary>
public class Gtid : IGtid
{
    /// <summary>
    /// Gets domain identifier in multi-master setup.
    /// </summary>
    public long DomainId { get; }

    /// <summary>
    /// Gets identifier of the server that generated the event.
    /// </summary>
    public long ServerId { get; }

    /// <summary>
    /// Gets sequence number of the event on the original server.
    /// </summary>
    public long Sequence { get; }

    /// <summary>
    /// Creates a new <see cref="Gtid"/>.
    /// </summary>
    public Gtid(long domainId, long serverId, long sequence)
    {
        DomainId = domainId;
        ServerId = serverId;
        Sequence = sequence;
    }

    /// <summary>
    /// Returns string representation of Gtid in MariaDB.
    /// </summary>
    public override string ToString() => $"{DomainId}-{ServerId}-{Sequence}";
}