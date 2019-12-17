using MySql.Cdc.Constants;

namespace MySql.Cdc
{
    public class BinlogOptions
    {
        /// <summary>
        /// Binary log file name.
        /// The value is automatically changed on the RotateEvent.
        /// On reconnect the client resumes replication from the current position.
        /// </summary>
        public string Filename { get; set; }

        /// <summary>
        /// Binary log file position.
        /// The value is automatically changed when an event is successfully processed by a client.
        /// On reconnect the client resumes replication from the current position.
        /// </summary>
        public long Position { get; set; }

        public StartingStrategy StartingStrategy { get; private set; }

        private BinlogOptions() { }

        /// <summary>
        /// Starts replication from first available binlog on master server.
        /// </summary>
        public static BinlogOptions FromStart()
        {
            return new BinlogOptions
            {
                StartingStrategy = StartingStrategy.FromStart,
                Position = EventConstants.FirstEventPosition
            };
        }

        /// <summary>
        /// Starts replication from last master binlog position.
        /// </summary>
        public static BinlogOptions FromEnd()
        {
            return new BinlogOptions { StartingStrategy = StartingStrategy.FromEnd };
        }

        /// <summary>
        /// Starts replication from specified binlog filename and position.
        /// </summary>
        public static BinlogOptions FromPosition(string filename, long position)
        {
            return new BinlogOptions
            {
                StartingStrategy = StartingStrategy.FromPosition,
                Filename = filename,
                Position = position
            };
        }
    }
}
