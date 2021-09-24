using MySqlCdc.Constants;
using MySqlCdc.Events;

namespace MySqlCdc
{
    /// <summary>
    /// Replication options used when client connects to the server.
    /// </summary>
    public class BinlogOptions
    {
        /// <summary>
        /// Binary log file name.
        /// The value is automatically changed on the RotateEvent.
        /// On reconnect the client resumes replication from the current position.
        /// </summary>
        public string Filename { get; set; } = string.Empty;

        /// <summary>
        /// Binary log file position.
        /// The value is automatically changed when an event is successfully processed by a client.
        /// On reconnect the client resumes replication from the current position.
        /// </summary>
        public long Position { get; set; }

        /// <summary>
        /// Global Transaction ID position to start replication from.
        /// See <a href="https://dev.mysql.com/doc/refman/8.0/en/replication-gtids-concepts.html">MySQL GTID</a>
        /// See <a href="https://mariadb.com/kb/en/library/gtid/">MariaDB GTID</a>
        /// </summary>
        public IGtidState? GtidState { get; set; }

        /// <summary>
        /// Gets replication starting strategy.
        /// </summary>
        internal StartingStrategy StartingStrategy { get; }

        private BinlogOptions(StartingStrategy startingStrategy)
        {
            StartingStrategy = startingStrategy;
        }

        /// <summary>
        /// Starts replication from first available binlog on master server.
        /// </summary>
        public static BinlogOptions FromStart()
        {
            return new BinlogOptions(StartingStrategy.FromStart)
            {
                Position = EventConstants.FirstEventPosition
            };
        }

        /// <summary>
        /// Starts replication from last master binlog position
        /// which will be read by BinlogClient on first connect.
        /// </summary>
        public static BinlogOptions FromEnd()
        {
            return new BinlogOptions(StartingStrategy.FromEnd);
        }

        /// <summary>
        /// Starts replication from specified binlog filename and position.
        /// </summary>
        public static BinlogOptions FromPosition(string filename, long position)
        {
            return new BinlogOptions(StartingStrategy.FromPosition)
            {
                Filename = filename,
                Position = position
            };
        }

        /// <summary>
        /// Starts replication from specified Global Transaction ID.
        /// GTID format is different depending on whether you use MySQL or MariaDB.
        /// </summary>
        public static BinlogOptions FromGtid(IGtidState gtidState)
        {
            return new BinlogOptions(StartingStrategy.FromGtid)
            {
                GtidState = gtidState,
                Position = EventConstants.FirstEventPosition
            };
        }
    }
}
