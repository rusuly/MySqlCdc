namespace MySqlCdc.Events;

    /// <summary>
    /// Represents two seed values that set the rand_seed1 and rand_seed2 system variables that are used to compute the random number.
    /// <a href="https://mariadb.com/kb/en/rand_event/">See more</a>
    /// </summary>
    /// <remarks>
    /// Creates a new <see cref="RandEvent"/>.
    /// </remarks>
    public record RandEvent(ulong Seed1, ulong Seed2) : IBinlogEvent
    {
        /// <summary>
        /// Gets the rand_seed1
        /// </summary>
        public ulong Seed1 {get; } = Seed1;

        /// <summary>
        /// Gets the rand_seed2
        /// </summary>
        public ulong Seed2 { get; } = Seed2;
    }