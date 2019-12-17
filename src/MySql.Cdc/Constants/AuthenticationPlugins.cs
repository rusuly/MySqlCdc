namespace MySql.Cdc.Constants
{
    public static class AuthenticationPlugins
    {
        /// <summary>
        /// Used by default in MariaDB and MySQL 5.7 Server and prior.
        /// </summary>
        public const string MySqlNativePassword = "mysql_native_password";

        /// <summary>
        /// Used by default in MySQL Server 8.0.
        /// </summary>
        public const string CachingSha2Password = "caching_sha2_password";
    }
}
