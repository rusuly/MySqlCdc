namespace MySqlCdc.Events
{
    /// <summary>
    /// Represents the commit event of a prepared XA transaction.
    /// </summary>
    public class XaPrepareEvent : BinlogEvent
    {
        public bool OnePhase { get; }
        public int FormatId { get; }        
        public string Gtrid { get; }
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
}
