using System;
using MySqlCdc.Providers.MySql;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class GtidSetTests
    {
        private const string ServerUuid1 = "24bc7850-2c16-11e6-a073-0242ac110001";
        private const string ServerUuid2 = "24bc7850-2c16-11e6-a073-0242ac110002";

        [Fact]
        public void Test_ParseEmptyString_ReturnsEmptyGtidSet()
        {
            var gtidSet = GtidSet.Parse("");

            Assert.Empty(gtidSet.UuidSets.Values);
            Assert.Equal("", gtidSet.ToString());
        }

        [Fact]
        public void Test_AddGtids_GtidsMerged()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:3-5");

            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid1), 2));
            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid1), 4));
            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid1), 5));
            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid1), 7));
            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid2), 9));
            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid1), 0));

            Assert.Equal($"{ServerUuid1}:0:2-5:7,{ServerUuid2}:9", gtidSet.ToString());
        }

        [Fact]
        public void Test_AddGtidInGap_IntervalsJoined()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:3-4:6-7");
            gtidSet.AddGtid(new Gtid(Uuid.Parse(ServerUuid1), 5));

            Assert.Equal($"{ServerUuid1}:3-7", gtidSet.ToString());
        }

        [Fact]
        public void Test_RawGtidSets_EqualsCorrectly()
        {
            Assert.Equal(GtidSet.Parse(""), GtidSet.Parse(""));
            Assert.Equal(GtidSet.Parse($"{ServerUuid1}:1-191"), GtidSet.Parse($"{ServerUuid1}:1-191"));
            Assert.Equal(GtidSet.Parse($"{ServerUuid1}:1-191:192-199"), GtidSet.Parse($"{ServerUuid1}:1-191:192-199"));
            Assert.Equal(GtidSet.Parse($"{ServerUuid1}:1-191:192-199"), GtidSet.Parse($"{ServerUuid1}:1-199"));
            Assert.Equal(GtidSet.Parse($"{ServerUuid1}:1-191:193-199"), GtidSet.Parse($"{ServerUuid1}:1-191:193-199"));
            Assert.NotEqual(GtidSet.Parse($"{ServerUuid1}:1-191:193-199"), GtidSet.Parse($"{ServerUuid1}:1-199"));
        }

        [Fact]
        public void Test_SimpleGtidSet_HasSingleInterval()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:1-191");
            var uuidSet = gtidSet.UuidSets[Uuid.Parse(ServerUuid1)];

            Assert.Single(uuidSet.Intervals);
            Assert.Equal(new Interval(1, 191), uuidSet.Intervals[0]);
            Assert.Equal($"{ServerUuid1}:1-191", gtidSet.ToString());
        }

        [Fact]
        public void Test_СontinuousIntervals_Collapsed()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:1-191:192-199");
            var uuidSet = gtidSet.UuidSets[Uuid.Parse(ServerUuid1)];

            Assert.Single(uuidSet.Intervals);
            Assert.Equal(new Interval(1, 199), uuidSet.Intervals[0]);
            Assert.Equal($"{ServerUuid1}:1-199", gtidSet.ToString());
        }

        [Fact]
        public void Test_NonСontinuousIntervals_NotCollapsed()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:1-191:193-199");
            var uuidSet = gtidSet.UuidSets[Uuid.Parse(ServerUuid1)];

            Assert.Equal(2, uuidSet.Intervals.Count);
            Assert.Equal(new Interval(1, 191), uuidSet.Intervals[0]);
            Assert.Equal(new Interval(193, 199), uuidSet.Intervals[1]);
            Assert.Equal($"{ServerUuid1}:1-191:193-199", gtidSet.ToString());
        }

        [Fact]
        public void Test_MultipleIntervals_NotCollapsed()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:1-191:193-199:1000-1033");
            var uuidSet = gtidSet.UuidSets[Uuid.Parse(ServerUuid1)];

            Assert.Equal(3, uuidSet.Intervals.Count);
            Assert.Equal(new Interval(1, 191), uuidSet.Intervals[0]);
            Assert.Equal(new Interval(193, 199), uuidSet.Intervals[1]);
            Assert.Equal(new Interval(1000, 1033), uuidSet.Intervals[2]);
            Assert.Equal($"{ServerUuid1}:1-191:193-199:1000-1033", gtidSet.ToString());
        }

        [Fact]
        public void Test_MultipleIntervals_SomeCollapsed()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:1-191:192-199:1000-1033:1035-1036:1038-1039");
            var uuidSet = gtidSet.UuidSets[Uuid.Parse(ServerUuid1)];

            Assert.Equal(4, uuidSet.Intervals.Count);
            Assert.Equal(new Interval(1, 199), uuidSet.Intervals[0]);
            Assert.Equal(new Interval(1000, 1033), uuidSet.Intervals[1]);
            Assert.Equal(new Interval(1035, 1036), uuidSet.Intervals[2]);
            Assert.Equal(new Interval(1038, 1039), uuidSet.Intervals[3]);
            Assert.Equal($"{ServerUuid1}:1-199:1000-1033:1035-1036:1038-1039", gtidSet.ToString());
        }

        [Fact]
        public void Test_MultiServerSetup_HasSingleIntervalsTrimsSpaces()
        {
            var gtidSet = GtidSet.Parse($"{ServerUuid1}:1-3:11:47-49, {ServerUuid2}:1-19:55:56-100");

            Assert.Equal(2, gtidSet.UuidSets.Values.Count);
            var uuidSet1 = gtidSet.UuidSets[Uuid.Parse(ServerUuid1)];
            var uuidSet2 = gtidSet.UuidSets[Uuid.Parse(ServerUuid2)];

            Assert.Equal(3, uuidSet1.Intervals.Count);
            Assert.Equal(new Interval(1, 3), uuidSet1.Intervals[0]);
            Assert.Equal(new Interval(11, 11), uuidSet1.Intervals[1]);
            Assert.Equal(new Interval(47, 49), uuidSet1.Intervals[2]);

            Assert.Equal(2, uuidSet2.Intervals.Count);
            Assert.Equal(new Interval(1, 19), uuidSet2.Intervals[0]);
            Assert.Equal(new Interval(55, 100), uuidSet2.Intervals[1]);

            Assert.Equal($"{ServerUuid1}:1-3:11:47-49,{ServerUuid2}:1-19:55-100", gtidSet.ToString());
        }
    }
}
