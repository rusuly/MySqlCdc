namespace MySqlCdc.Constants
{
    internal class EventConstants
    {
        public const int HeaderSize = 19;
        public const int FirstEventPosition = 4;
        public const string TableMapNotFound = "No preceding TableMapEvent event was found for the row event. You possibly started replication in the middle of logical event group.";
    }
}
