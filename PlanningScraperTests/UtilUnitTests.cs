using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanningScraper.Utils;

namespace PlanningScraperTests
{
    [TestClass]
    public class UtilUnitTests
    {
        [TestMethod]
        public async Task GivenADateRange_WhenEndDateBeforeFirstChunkEndDate_ThenShouldReturnOneChunkUsingEndDate()
        {
            var dateRanges = await DateChunker.SplitDateRange("01-03-2018", "15-03-2018", 30);
            Assert.AreEqual(1, dateRanges.Count);
            Assert.IsTrue(dateRanges.ContainsKey("01-03-2018"));
            Assert.AreEqual("15-03-2018", dateRanges["01-03-2018"]);
        }

        [TestMethod]
        public async Task GivenADateRange_WhenEndDateIsSameAsChunkEndDate_ThenShouldReturnOneChunkUsingEndDate()
        {
            var dateRanges = await DateChunker.SplitDateRange("01-03-2018", "30-03-2018", 30);
            Assert.AreEqual(1, dateRanges.Count);
            Assert.IsTrue(dateRanges.ContainsKey("01-03-2018"));
            Assert.AreEqual("30-03-2018", dateRanges["01-03-2018"]);
        }

        [TestMethod]
        public async Task GivenADateRange_WhenEndDateIsGreaterThanChunkEndDate_ThenShouldReturnTwoChunksSecondChunkBeingSmaller()
        {
            var dateRanges = await DateChunker.SplitDateRange("01-03-2018", "15-04-2018", 30);
            Assert.AreEqual(2, dateRanges.Count);
            Assert.IsTrue(dateRanges.ContainsKey("01-03-2018"));
            Assert.AreEqual("30-03-2018", dateRanges["01-03-2018"]);
            Assert.AreEqual("31-03-2018", dateRanges.Keys.Last());
            Assert.AreEqual("15-04-2018", dateRanges.Values.Last());
        }

        [TestMethod]
        public async Task GivenADateRange_WhenEndDateIsGreaterThanChunkEndDate_ThenShouldReturnTwoCFourChunksExactly()
        {
            var dateRanges = await DateChunker.SplitDateRange("01-03-2018", "28-06-2018", 30);
            Assert.AreEqual(4, dateRanges.Count);
            Assert.IsTrue(dateRanges.ContainsKey("01-03-2018"));
            Assert.AreEqual("30-03-2018", dateRanges["01-03-2018"]);
            Assert.IsTrue(dateRanges.ContainsKey("31-03-2018"));
            Assert.AreEqual("29-04-2018", dateRanges["31-03-2018"]);
            Assert.IsTrue(dateRanges.ContainsKey("30-04-2018"));
            Assert.AreEqual("29-05-2018", dateRanges["30-04-2018"]);
            Assert.AreEqual("30-05-2018", dateRanges.Keys.Last());
            Assert.AreEqual("28-06-2018", dateRanges.Values.Last());
        }

    }
}
