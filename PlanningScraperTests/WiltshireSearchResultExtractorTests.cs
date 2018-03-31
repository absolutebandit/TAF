using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanningScraper;
using PlanningScraper.Wiltshire;

namespace PlanningScraperTests
{
    [TestClass]
    public class WiltshireSearchResultExtractorTests
    {
        private static CQ _searchPageResponseDoc;
        private static CQ _planningApplicationDoc;
        private static CQ _documentSearchResultsDoc;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var searchResultsHtml = File.ReadAllText("WiltshireSamples\\SearchResultsSample.html");
            _searchPageResponseDoc = CQ.Create(searchResultsHtml);

            var planningApplicationHtml = File.ReadAllText("WiltshireSamples\\PlanningApplicationSample.html");
            _planningApplicationDoc = CQ.Create(planningApplicationHtml);

            var documentSearchResultsHtml = File.ReadAllText("WiltshireSamples\\DocumentSearchResults.html");
            _documentSearchResultsDoc = CQ.Create(documentSearchResultsHtml);
        }

        [TestMethod]
        public void GivenAWiltshireSearchResultDocument_CanExtractDetails()
        {
            // Arrange
            var planningApplications = new List<PlanningApplication>();

            // Act
            _searchPageResponseDoc.Select("table tbody tr").Each(tr =>
            {
                var planningApplication = new PlanningApplication();
               
                WiltshireExtractor.GetSeachRowDetailAsync(tr,planningApplication, CancellationToken.None).GetAwaiter().GetResult();

                planningApplications.Add(planningApplication);

                // Assert
                Assert.IsNotNull(planningApplication.ApplicationReference);
                Assert.IsNotNull(planningApplication.ApplicationLink);
                Assert.IsNotNull(planningApplication.SiteAddress);
                Assert.IsNotNull(planningApplication.Proposal);
                Assert.AreNotEqual(string.Empty, planningApplication.ApplicationReference);
                Assert.AreNotEqual(string.Empty, planningApplication.ApplicationLink);
                Assert.AreNotEqual(string.Empty, planningApplication.SiteAddress);
                Assert.AreNotEqual(string.Empty, planningApplication.Proposal);
            });

            Assert.AreEqual(143, planningApplications.Count);
        }

        [TestMethod]
        public async Task GivenAWiltshireApplicationPage_CanExtractDetails()
        {
            // Arrange
            var planningApplication = new PlanningApplication();

            // Act 
            await WiltshireExtractor.ExtractPlanningApplicationData(planningApplication, _planningApplicationDoc);

            // Assert
            Assert.IsNotNull(planningApplication.RegisteredDate);
            Assert.IsNotNull(planningApplication.ConsultationExpiryDate);
            Assert.IsNotNull(planningApplication.TargetDate);
            Assert.IsNotNull(planningApplication.ApplicationType);
            Assert.IsNotNull(planningApplication.CurrentStatus);
            Assert.IsNotNull(planningApplication.NameOfApplicant);
            Assert.IsNotNull(planningApplication.AgentName);
            Assert.IsNotNull(planningApplication.Wards);
            Assert.IsNotNull(planningApplication.Parishes);
            Assert.IsNotNull(planningApplication.CaseOfficer);
            Assert.IsNotNull(planningApplication.DocumentsLink);

            Assert.AreNotEqual(string.Empty, planningApplication.RegisteredDate);
            Assert.AreNotEqual(string.Empty, planningApplication.ConsultationExpiryDate);
            Assert.AreNotEqual(string.Empty, planningApplication.TargetDate);
            Assert.AreNotEqual(string.Empty, planningApplication.ApplicationType);
            Assert.AreNotEqual(string.Empty, planningApplication.CurrentStatus);
            Assert.AreNotEqual(string.Empty, planningApplication.NameOfApplicant);
            Assert.AreNotEqual(string.Empty, planningApplication.AgentName);
            Assert.AreNotEqual(string.Empty, planningApplication.Wards);
            Assert.AreNotEqual(string.Empty, planningApplication.Parishes);
            Assert.AreNotEqual(string.Empty, planningApplication.CaseOfficer);
            Assert.AreNotEqual(string.Empty, planningApplication.DocumentsLink);
        }

        [TestMethod]
        public async Task GivenWiltshireDocumentSearchResults_CanExtractDocumentLinks()
        {
            // Arrange
            var planningApplication = new PlanningApplication();

            // Act
            await WiltshireExtractor.ExtractDocumentLinksAsync(planningApplication, _documentSearchResultsDoc, CancellationToken.None);

            // Assert
            Assert.IsNotNull(planningApplication.AllDocumentLinks);
            Assert.AreNotEqual(string.Empty, planningApplication.AllDocumentLinks);
        }
    }
}
