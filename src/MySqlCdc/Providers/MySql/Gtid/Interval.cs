namespace MySqlCdc.Providers.MySql;

/// <summary>
/// Represents contiguous transaction interval in GtidSet.
/// </summary>
public class Interval
{
    /// <summary>
    /// Gets first transaction id in the interval.
    /// </summary>
    public long Start { get; set; }

    /// <summary>
    /// Gets last transaction id in the interval.
    /// </summary>
    public long End { get; set; }

    /// <summary>
    /// Creates a new <see cref="Interval"/>.
    /// </summary>
    public Interval(long start, long end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Compares two values for equality
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (this == obj)
            return true;

        if (obj is Interval other)
            return Start == other.Start && End == other.End;

        return false;
    }

    /// <summary>
    /// Calls to this method always throw a <see cref="NotSupportedException"/>.
    /// </summary>
    public override int GetHashCode() => throw new NotSupportedException();

    /// <summary>
    /// Returns string representation of an UuidSet interval.
    /// </summary>
    public override string ToString()
    {
        if (Start == End)
            return Start.ToString();

        return $"{Start}-{End}";
    }
}