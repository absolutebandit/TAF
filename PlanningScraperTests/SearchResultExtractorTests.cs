using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CsQuery;
using CsQuery.EquationParser.Implementation;
using CsQuery.Implementation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PlanningScraper;

namespace PlanningScraperTests
{
    [TestClass]
    public class SearchResultExtractorTests
    {
        private static CsQuery.CQ _searchPageResponseDoc;
        private static CsQuery.CQ _planningApplicationDoc;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            var searchResultsHtml = File.ReadAllText("SearchResultsSample.html");
            _searchPageResponseDoc = CsQuery.CQ.Create(searchResultsHtml);

            var planningApplicationHtml = File.ReadAllText("PlanningApplicationSample.html");
            _planningApplicationDoc = CsQuery.CQ.Create(planningApplicationHtml);
        }

        [TestMethod]
        public void GivenASearchResultDocument_CanExtractDetails()
        {
            var planningApplications = new List<PlanningApplication>();

            var row = 0;
            var test1 = _searchPageResponseDoc.Select("#ResultsPaginationTop").Select("tbody").Select("tr");
            _searchPageResponseDoc.Select("#ResultsPaginationTop").Select("tbody").Select("tr").Each(tr =>
            {
                if (row > 0)
                {
                    var planningApplication = new PlanningApplication();
                    var cellPos = 0;
                    foreach (var cell in tr.ChildElements)
                    {
                        switch (cellPos)
                        {
                            // Application
                            case 0:
                                planningApplication.ApplicationReference = cell.InnerText;
                                planningApplication.ApplicationLink = cell.FirstElementChild.Attributes["href"];
                                break;
                            
                            // Site
                            case 1:
                                planningApplication.SiteAddress = cell.InnerText;
                                break;

                            // Proposal
                            case 2:
                                planningApplication.Proposal = cell.InnerText;
                                break;
                        }
                        cellPos++;
                    }

                    planningApplications.Add(planningApplication);

                    Assert.IsNotNull(planningApplication.ApplicationReference);
                    Assert.IsNotNull(planningApplication.ApplicationLink);
                    Assert.IsNotNull(planningApplication.SiteAddress);
                    Assert.IsNotNull(planningApplication.Proposal);
                    Assert.AreNotEqual(string.Empty, planningApplication.ApplicationReference);
                    Assert.AreNotEqual(string.Empty, planningApplication.ApplicationLink);
                    Assert.AreNotEqual(string.Empty, planningApplication.SiteAddress);
                    Assert.AreNotEqual(string.Empty, planningApplication.Proposal);
                }
                row++;
            });

            Assert.AreEqual(143, planningApplications.Count);
        }

        [TestMethod]
        public void GivenAnApplicationPage_CanExtractDetails()
        {

        }
    }
}
