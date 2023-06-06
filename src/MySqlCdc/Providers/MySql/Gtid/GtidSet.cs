using MySqlCdc.Events;

namespace MySqlCdc.Providers.MySql;

/// <summary>
/// Represents GtidSet from MySQL 5.6 and above.
/// <a href="https://dev.mysql.com/doc/refman/8.0/en/replication-gtids-concepts.html">See more</a>
/// </summary>
public class GtidSet : IGtidState
{
    /// <summary>
    /// Gets a list of UuidSet parts in the GtidSet.
    /// </summary>
    public Dictionary<Uuid, UuidSet> UuidSets { get; } = new Dictionary<Uuid, UuidSet>();

    /// <summary>
    /// Parses <see cref="GtidSet"/> from string representation.
    /// </summary>
    public static GtidSet Parse(string gtidSet)
    {
        if (gtidSet == string.Empty)
            return new GtidSet();

        var uuidSets = gtidSet.Replace("\n", string.Empty)
            .Split(',')
            .Select(x => x.Trim())
            .ToArray();

        var result = new GtidSet();
        foreach (var uuidSet in uuidSets)
        {
            int separatorIndex = uuidSet.IndexOf(':');
            var sourceId = Uuid.Parse(uuidSet.Substring(0, separatorIndex));

            var intervals = new List<Interval>();
            string[] ranges = uuidSet.Substring(separatorIndex + 1).Split(':');

            foreach (var token in ranges)
            {
                string[] range = token.Split('-');
                var interval = range.Length switch
                {
                    1 => new Interval(long.Parse(range[0]), long.Parse(range[0])),
                    2 => new Interval(long.Parse(range[0]), long.Parse(range[1])),
                    _ => throw new FormatException($"Invalid interval format {token}")
                };
                intervals.Add(interval);
            }

            result.UuidSets[sourceId] = new UuidSet(sourceId, intervals);
        }
        return result;
    }

    /// <summary>
    /// Adds a gtid value to the GtidSet.
    /// </summary>
    public bool AddGtid(IGtid value)
    {
        var gtid = (Gtid)value;

        if (!UuidSets.TryGetValue(gtid.SourceId, out var uuidSet))
        {
            uuidSet = new UuidSet(gtid.SourceId, new List<Interval>());
            UuidSets[gtid.SourceId] = uuidSet;
        }
        return uuidSet.AddGtid(gtid);
    }

    /// <summary>
    /// Compares two values for equality
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (this == obj)
            return true;

        if (obj is GtidSet other)
            return UuidSets.Values.SequenceEqual(other.UuidSets.Values);

        return false;
    }

    /// <summary>
    /// Calls to this method always throw a <see cref="NotSupportedException"/>.
    /// </summary>
    public override int GetHashCode() => throw new NotSupportedException();

    /// <summary>
    /// Returns string representation of the GtidSet.
    /// </summary>
    public override string ToString()
    {
        var uuids = UuidSets.Values
            .OrderBy(x => x.SourceId.ToString())
            .ToList();
        return string.Join(",", uuids);
    }
}