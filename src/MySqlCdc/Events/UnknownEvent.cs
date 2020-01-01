namespace MySqlCdc.Events
{
    public class UnknownEvent : BinlogEvent
    {
        public UnknownEvent(EventHeader header) : base(header)
        {
        }
    }
}
