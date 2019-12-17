namespace MySql.Cdc.Events
{
    public abstract class BinlogEvent : IBinlogEvent
    {
        public EventHeader Header { get; private set; }

        protected BinlogEvent(EventHeader header)
        {
            Header = header;
        }
    }
}
