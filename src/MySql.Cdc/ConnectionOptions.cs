using System;

namespace MySql.Cdc
{
    /// <summary>
    /// Settings used to connect to MySql server.
    /// </summary>
    public class ConnectionOptions
    {
        /// <summary>
        /// MySQL/MariaDB port number to connect to. Defaults to 3306.
        /// </summary>
        public int Port { get; set; } = 3306;

        /// <summary>
        /// Defines whether SSL/TLS must be used. Defaults to false.
        /// </summary>
        public bool UseSsl { get; set; } = false;

        /// <summary>
        /// A database user which is used to register as a database slave.
        /// The user needs to have <c>REPLICATION SLAVE</c>, <c>REPLICATION CLIENT</c> privileges.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The password of the user which is used to connect.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Default database name specified in Handshake connection.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Specifies the slave server id and used only in blocking mode. Defaults to 1.
        /// <see cref="https://dev.mysql.com/doc/refman/8.0/en/mysqlbinlog-server-id.html"/>
        /// </summary>
        public long ServerId { get; set; } = 1;

        /// <summary>
        /// Specifies whether to stream events or read until last event and then return. 
        /// Defaults to true (stream events and wait for new ones).
        /// </summary>
        public bool Blocking { get; set; } = true;

        /// <summary>
        /// Defines the binlog coordinates that replication should start from.
        /// Defaults to BinlogOptions.FromEnd()
        /// </summary>
        public BinlogOptions Binlog = BinlogOptions.FromEnd();

        /// <summary>
        /// Defines interval of keep alive messages that the master sends to the slave. 
        /// Defaults to 30 seconds.
        /// </summary>
        public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);
    }
}
