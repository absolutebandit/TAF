using System.IO;
using CsQuery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanningScraper;
using PlanningScraper.Extensions;

namespace PlanningScraperTests
{
    [TestClass]
    public class IdoxSearchResultExtractorTests
    {
        private static CQ _searchPageResponseDoc;
        private static CQ _applicationSummaryResponseDoc;
        private static CQ _applicationDetailsResponseDoc;
        private static CQ _applicationContactsResponseDoc;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var searchResultsHtml = File.ReadAllText("IdoxSamples\\IdoxSearchResultsPage.html");
            _searchPageResponseDoc = CQ.Create(searchResultsHtml);

            var applicationSummaryHtml = File.ReadAllText("IdoxSamples\\IdoxApplicationSummarySample.html");
            _applicationSummaryResponseDoc = CQ.Create(applicationSummaryHtml);

            var applicationDetailsHtml = File.ReadAllText("IdoxSamples\\IdoxApplicationDetailsSample.html");
            _applicationDetailsResponseDoc = CQ.Create(applicationDetailsHtml);

            var applicationContactsHtml = File.ReadAllText("IdoxSamples\\IdoxApplicationContactsSample.html");
            _applicationContactsResponseDoc = CQ.Create(applicationContactsHtml);
        }

        [TestMethod]
        public void GivenAIdoxSearchResultDocument_CanExtractApplicationLinks()
        {
            // Act
            var links = _searchPageResponseDoc.Select(".searchresult a");
            foreach (IDomObject link in links)
            {
                var href = link.Attributes["href"];
                Assert.IsNotNull(href);
                Assert.AreNotEqual(string.Empty, href);
            }
        }

        [TestMethod]
        public void GivenAIdoxApplicationSummaryResponse_CanExtractApplicationSummaryDetails()
        {
            var planningApplication = new PlanningApplication();

            _applicationSummaryResponseDoc.Select("#simpleDetailsTable tr").Each(row =>
            {
                if (row.ChildNodes[1].InnerText.Contains("Reference") && !row.ChildNodes[1].InnerText.Contains("Alternative Reference"))
                {
                    planningApplication.ApplicationReference = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Application Validated"))
                {
                    planningApplication.RegisteredDate = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Address"))
                {
                    planningApplication.SiteAddress = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Proposal"))
                {
                    planningApplication.Proposal = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Status") && !row.ChildNodes[1].InnerText.Contains("Appeal Status"))
                {
                    planningApplication.CurrentStatus = row.ChildNodes[3].InnerText.Clean();
                }
            });

            if (string.IsNullOrEmpty(planningApplication.Proposal))
            {
                planningApplication.Proposal = _applicationSummaryResponseDoc.Select(".description").Text().Clean();
            }

            if (string.IsNullOrEmpty(planningApplication.SiteAddress))
            {
                planningApplication.SiteAddress = _applicationSummaryResponseDoc.Select(".address").Text().Clean();
            }

            Assert.IsNotNull(planningApplication.ApplicationReference);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.ApplicationReference));
            Assert.IsNotNull(planningApplication.RegisteredDate);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.RegisteredDate));
            Assert.IsNotNull(planningApplication.SiteAddress);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.SiteAddress));
            Assert.IsNotNull(planningApplication.Proposal);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.Proposal));
            Assert.IsNotNull(planningApplication.CurrentStatus);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.CurrentStatus));
        }

        [TestMethod]
        public void GivenAIdoxApplicationDetailsResponse_CanExtractApplicationDetails()
        {
            var planningApplication = new PlanningApplication();

            _applicationDetailsResponseDoc.Select("#applicationDetails tr").Each(row =>
            {
                if (row.ChildNodes[1].InnerText.Contains("Application Type"))
                {
                    planningApplication.ApplicationType = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Case Officer"))
                {
                    planningApplication.CaseOfficer = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Parish"))
                {
                    planningApplication.Parishes = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Ward"))
                {
                    planningApplication.Wards = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Applicant Name"))
                {
                    planningApplication.NameOfApplicant = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Agent Name"))
                {
                    planningApplication.AgentName = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Agent Company Name"))
                {
                    planningApplication.AgentCompanyName = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Agent Address"))
                {
                    planningApplication.AgentAddress = row.ChildNodes[3].InnerText.Clean();
                }

                if (row.ChildNodes[1].InnerText.Contains("Agent Phone Number"))
                {
                    planningApplication.AgentPhoneNumber = row.ChildNodes[3].InnerText.Clean();
                }
            });

            Assert.IsNotNull(planningApplication.ApplicationType);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.ApplicationType));
            Assert.IsNotNull(planningApplication.CaseOfficer);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.CaseOfficer));
            Assert.IsNotNull(planningApplication.Parishes);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.Parishes));
            Assert.IsNotNull(planningApplication.Wards);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.Wards));
            Assert.IsNotNull(planningApplication.NameOfApplicant);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.NameOfApplicant));
            Assert.IsNotNull(planningApplication.AgentName);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.AgentName));
            Assert.IsNotNull(planningApplication.AgentCompanyName);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.AgentCompanyName));
            Assert.IsNotNull(planningApplication.AgentAddress);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.AgentAddress));
            Assert.IsNotNull(planningApplication.AgentPhoneNumber);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.AgentPhoneNumber));
        }

        [TestMethod]
        public void GivenAIdoxApplicationContactsResponse_CanExtractAgentEmail()
        {
            var planningApplication = new PlanningApplication();

            _applicationContactsResponseDoc.Select(".agents tr").Each(row =>
            {
                if ((string.IsNullOrEmpty(planningApplication.AgentEmail) || planningApplication.AgentEmail == "Not Available") &&
                    row.ChildNodes[1].InnerText.ToLower().Contains("email"))
                {
                    planningApplication.AgentEmail = row.ChildNodes[3].InnerText.Clean();
                }

                if ((string.IsNullOrEmpty(planningApplication.AgentPhoneNumber) || planningApplication.AgentEmail == "Not Available") &&
                    row.ChildNodes[1].InnerText.ToLower().Contains("phone"))
                {
                    planningApplication.AgentPhoneNumber = row.ChildNodes[3].InnerText.Clean();
                }
            });

            Assert.IsNotNull(planningApplication.AgentEmail);
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.AgentEmail));
        }
    }
}
