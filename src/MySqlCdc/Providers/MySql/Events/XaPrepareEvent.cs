namespace MySqlCdc.Events;

/// <summary>
/// Represents the commit event of a prepared XA transaction.
/// </summary>
public class XaPrepareEvent : BinlogEvent
{
    /// <summary>
    /// XA transaction commit type. False => XA PREPARE. True => XA COMMIT ... ONE PHASE
    /// </summary>
    public bool OnePhase { get; }

    /// <summary>
    /// The formatID part of the transaction xid.
    /// </summary>
    public int FormatId { get; }

    /// <summary>
    /// A global transaction identifier.
    /// </summary>  
    public string Gtrid { get; }

    /// <summary>
    /// A branch qualifier.
    /// </summary>  
    public string Bqual { get; }

    /// <summary>
    /// Creates a new <see cref="XaPrepareEvent"/>.
    /// </summary>
    public XaPrepareEvent(EventHeader header, bool onePhase, int formatId, string gtrid, string bqual) : base(header)
    {
        OnePhase = onePhase;
        FormatId = formatId;
        Gtrid = gtrid;
        Bqual = bqual;
    }
}