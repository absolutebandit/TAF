using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanningScraper;
using PlanningScraper.Extensions;
using PlanningScraper.Wiltshire;

namespace PlanningScraperTests
{
    [TestClass]
    public class BournemouthSearchResultExtractorTests
    {
        private static CQ _bournemouthSearchResultsPageDoc;
        private static CQ _bournemouthMainDetailsPageDoc;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var searchResultsHtml = File.ReadAllText("BournemouthSamples\\BournemouthSearchResultsPage.html");
            _bournemouthSearchResultsPageDoc = CQ.Create(searchResultsHtml);

            var mainDetailtsHtml = File.ReadAllText("BournemouthSamples\\BournemouthMainDetailsPage.html");
            _bournemouthMainDetailsPageDoc = CQ.Create(mainDetailtsHtml);
        }

        [TestMethod]
        public void GivenABournemouthSearchResultDocumentAndSearchTerm_CanExtractLinks()
        {

            // Arrange
            var index = 0;
            var searchTerm = "dwellinghouses";
            var appLinks = new List<string>();

            // Act
            _bournemouthSearchResultsPageDoc.Select("table tbody tr").Each(tr =>
            {
                if (tr.ChildNodes[7].InnerText.Contains(searchTerm))
                {
                    appLinks.Add(tr.ChildNodes[3].ChildNodes[1].Attributes["href"]);
                    Assert.IsFalse(string.IsNullOrEmpty(appLinks[index]));
                    index++;
                }
            });

            Assert.AreEqual(4, appLinks.Count);
        }

        [TestMethod]
        public void GivenABournemouthMainDetailsPage_CanExtractDetails()
        {
            var planningApplication = new PlanningApplication();
            _bournemouthMainDetailsPageDoc.Select("#MainContent_RadPageView1 .form tr").Each(tr =>
            {
                foreach (var childnodes in tr.ChildNodes)
                {
                    if (childnodes.HasChildren)
                    {
                        foreach (var child in childnodes.ChildNodes)
                        {
                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Application No")
                            {
                                planningApplication.ApplicationReference = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Type")
                            {
                                planningApplication.ApplicationType = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Applicant")
                            {
                                planningApplication.NameOfApplicant = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Agent")
                            {
                                planningApplication.AgentName = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Case Officer")
                            {
                                planningApplication.CaseOfficer = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Proposal")
                            {
                                planningApplication.Proposal = childnodes.NextSibling.NextSibling.ChildNodes[1].TextContent.Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Received Date")
                            {
                                planningApplication.RegisteredDate = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }
                        }
                    }
                }
            });

            _bournemouthMainDetailsPageDoc.Select("#MainContent_RadPageView2 .form tr").Each(tr =>
            {
                foreach (var childnodes in tr.ChildNodes)
                {
                    if (childnodes.HasChildren)
                    {
                        foreach (var child in childnodes.ChildNodes)
                        {
                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Address")
                            {
                                planningApplication.SiteAddress = childnodes.NextSibling.NextSibling.ChildNodes[1].TextContent.Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Ward")
                            {
                                planningApplication.Wards = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }

                            if (child.NodeType == NodeType.ELEMENT_NODE && child.InnerText == "Parish")
                            {
                                planningApplication.Parishes = childnodes.NextSibling.NextSibling.ChildNodes[1].Attributes["Value"].Clean();
                                break;
                            }
                        }
                    }
                }
            });

            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.ApplicationReference));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.AgentName));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.ApplicationType));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.ApplicationReference));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.CaseOfficer));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.NameOfApplicant));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.Proposal));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.RegisteredDate));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.SiteAddress));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.Wards));
            Assert.IsFalse(string.IsNullOrEmpty(planningApplication.Parishes));
        }

        [TestMethod]
        public void GivenABournemouthLocationPage_CanExtractDetails()
        {

        }
    }
}
