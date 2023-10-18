namespace MySqlCdc.Events;

/// <summary>
/// Represents the commit event of a prepared XA transaction.
/// </summary>
/// <remarks>
/// Creates a new <see cref="XaPrepareEvent"/>.
/// </remarks>
public record XaPrepareEvent(bool OnePhase, int FormatId, string Gtrid, string Bqual) : IBinlogEvent
{
    /// <summary>
    /// XA transaction commit type. False => XA PREPARE. True => XA COMMIT ... ONE PHASE
    /// </summary>
    public bool OnePhase { get; } = OnePhase;

    /// <summary>
    /// The formatID part of the transaction xid.
    /// </summary>
    public int FormatId { get; } = FormatId;

    /// <summary>
    /// A global transaction identifier.
    /// </summary>  
    public string Gtrid { get; } = Gtrid;

    /// <summary>
    /// A branch qualifier.
    /// </summary>  
    public string Bqual { get; } = Bqual;
}