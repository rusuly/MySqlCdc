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
        /// Defines whether SSL/TLS must be used.
        /// </summary>
        public bool UseSsl { get; set; }

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
    }
}
