using System;
using MySqlCdc.Providers.MariaDb;
using Xunit;

namespace MySqlCdc.Tests.Providers
{
    public class GtidListTests
    {
        [Fact]
        public void Test_ParseNullString_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => GtidList.Parse(null).ToString());
        }

        [Fact]
        public void Test_ParseNotUniqueDomains_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => GtidList.Parse("1-1-270, 1-1-271").ToString());
        }

        [Fact]
        public void Test_ParseEmptyString_ReturnsEmptyGtidList()
        {
            var gtidList = GtidList.Parse("");

            Assert.Equal(0, gtidList.Gtids.Count);
            Assert.Equal("", gtidList.ToString());
        }

        [Fact]
        public void Test_ParseGtidLists_ReturnsMultipleResults()
        {
            var gtidList1 = GtidList.Parse("0-1-270");
            var gtidList2 = GtidList.Parse("1-2-120,2-3-130");
            var gtidList3 = GtidList.Parse("1-2-120, 2-3-130, 3-4-50");

            Assert.Equal(1, gtidList1.Gtids.Count);
            Assert.Equal(2, gtidList2.Gtids.Count);
            Assert.Equal(3, gtidList3.Gtids.Count);

            Assert.Equal("0-1-270", gtidList1.ToString());
            Assert.Equal("1-2-120,2-3-130", gtidList2.ToString());
            Assert.Equal("1-2-120,2-3-130,3-4-50", gtidList3.ToString());
        }

        [Fact]
        public void Test_AddExistingDomainGtid_GtidUpdated()
        {
            var gtidList = GtidList.Parse("0-1-270");
            gtidList.AddGtid(new Gtid(0, 1, 271));

            Assert.Equal(1, gtidList.Gtids.Count);
            Assert.Equal("0-1-271", gtidList.ToString());
        }

        [Fact]
        public void Test_AddNewDomainGtid_GtidAdded()
        {
            var gtidList = GtidList.Parse("0-1-270");
            gtidList.AddGtid(new Gtid(1, 1, 271));

            Assert.Equal(2, gtidList.Gtids.Count);
            Assert.Equal("0-1-270,1-1-271", gtidList.ToString());
        }

        [Fact]
        public void Test_AddMultiDomainGtid_GtidMerged()
        {
            var gtidList = GtidList.Parse("1-2-120,2-3-130,3-4-50");
            gtidList.AddGtid(new Gtid(2, 4, 250));

            Assert.Equal(3, gtidList.Gtids.Count);
            Assert.Equal("1-2-120,2-4-250,3-4-50", gtidList.ToString());
        }
    }
}
