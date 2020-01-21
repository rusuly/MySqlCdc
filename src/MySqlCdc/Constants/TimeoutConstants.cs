namespace MySqlCdc.Constants
{
    internal class TimeoutConstants
    {
        /// <summary>
        /// Takes into account network latency.
        /// </summary>
        public const int Delta = 1000;

        public const string Message = "Could not receive a master heartbeat within the specified interval";
    }
}
