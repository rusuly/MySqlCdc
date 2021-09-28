using System;
using System.Collections.Generic;
using System.Linq;

namespace MySqlCdc.Providers.MySql;

/// <summary>
/// Represents replication state for a specific server.
/// </summary>
public class UuidSet
{
    /// <summary>
    /// Gets server uuid of the UuidSet.
    /// </summary>
    public Uuid SourceId { get; }

    /// <summary>
    /// Gets a list of intervals of the UuidSet.
    /// </summary>
    public List<Interval> Intervals { get; }

    /// <summary>
    /// Creates a new <see cref="UuidSet"/>.
    /// </summary>
    public UuidSet(Uuid sourceId, List<Interval> intervals)
    {
        SourceId = sourceId;
        Intervals = intervals;

        if (Intervals.Count > 1)
        {
            CollapseIntervals();
        }
    }

    /// <summary>
    /// Adds a gtid value to the UuidSet.
    /// </summary>
    public bool AddGtid(Gtid gtid)
    {
        if (SourceId != gtid.SourceId)
            throw new ArgumentException("SourceId of the passed gtid doesn't belong to the UuidSet");

        int index = FindIntervalIndex(gtid.TransactionId);
        bool added = false;
        if (index < Intervals.Count)
        {
            var interval = Intervals[index];
            if (interval.Start == gtid.TransactionId + 1)
            {
                interval.Start = gtid.TransactionId;
                added = true;
            }
            else if (interval.End + 1 == gtid.TransactionId)
            {
                interval.End = gtid.TransactionId;
                added = true;
            }
            else if (interval.Start <= gtid.TransactionId && gtid.TransactionId <= interval.End)
                return false;
        }
        if (!added)
        {
            Intervals.Insert(index, new Interval(gtid.TransactionId, gtid.TransactionId));
        }
        if (Intervals.Count > 1)
        {
            CollapseIntervals();
        }
        return true;
    }

    private int FindIntervalIndex(long transactionId)
    {
        int resultIndex = 0, leftIndex = 0, rightIndex = Intervals.Count;
        while (leftIndex < rightIndex)
        {
            resultIndex = (leftIndex + rightIndex) / 2;
            var interval = Intervals[resultIndex];
            if (interval.End < transactionId)
            {
                leftIndex = resultIndex + 1;
            }
            else if (transactionId < interval.Start)
            {
                rightIndex = resultIndex;
            }
            else return resultIndex;
        }
        if (Intervals.Any() && Intervals[resultIndex].End < transactionId)
        {
            resultIndex++;
        }
        return resultIndex;
    }

    private void CollapseIntervals()
    {
        int index = 0;
        while (index < Intervals.Count - 1)
        {
            Interval left = Intervals[index], right = Intervals[index + 1];
            if (left.End + 1 == right.Start)
            {
                left.End = right.End;
                Intervals.Remove(right);
            }
            else index++;
        }
    }

    /// <summary>
    /// Compares two values for equality
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (this == obj)
            return true;

        if (obj is UuidSet other)
            return SourceId == other.SourceId && Intervals.SequenceEqual(other.Intervals);

        return false;
    }

    /// <summary>
    /// Calls to this method always throw a <see cref="NotSupportedException"/>.
    /// </summary>
    public override int GetHashCode() => throw new NotSupportedException();

    /// <summary>
    /// Returns string representation of an UuidSet part of a GtidSet.
    /// </summary>
    public override string ToString() => $"{SourceId}:{string.Join(":", Intervals)}";
}