namespace MySql.Cdc.Events
{
    public abstract class BinlogEvent : IBinlogEvent
    {
        public EventHeader Header { get; }

        protected BinlogEvent(EventHeader header)
        {
            Header = header;
        }
    }
}
