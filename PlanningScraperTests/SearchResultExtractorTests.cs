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

            _searchPageResponseDoc.Select("table tbody tr").Each(tr =>
            {
                var planningApplication = new PlanningApplication();
                var cellPos = 0;
                foreach (var cell in tr.ChildElements)
                {
                    switch (cellPos)
                    {
                        // Application
                        case 0:
                            planningApplication.ApplicationReference = cell.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                            planningApplication.ApplicationLink = cell.FirstElementChild.Attributes["href"].Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", "; ");
                            break;
                            
                        // Site
                        case 1:
                            planningApplication.SiteAddress = cell.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", ", ").Replace("\n", string.Empty);
                            break;

                        // Proposal
                        case 2:
                            planningApplication.Proposal = cell.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
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
            });

            Assert.AreEqual(143, planningApplications.Count);
        }

        [TestMethod]
        public void GivenAnApplicationPage_CanExtractDetails()
        {
            var planningApplication = new PlanningApplication();

            _planningApplicationDoc.Select(".planappkd dl dt").Each(dt =>
            {
                switch (dt.InnerText)
                {
                    case "Registered (validated)":
                        planningApplication.RegisteredDate = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Consultation expiry":
                        planningApplication.ConsultationExpiryDate = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Target date for decision":
                        planningApplication.TargetDate = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;
                }
            });

            _planningApplicationDoc.Select("#wrapper dl:nth-of-type(2) dt").Each(dt =>
            {
                switch (dt.InnerText)
                {
                    case "Application type":
                        planningApplication.ApplicationType = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Current status of application":
                        planningApplication.CurrentStatus = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Name of applicant":
                        planningApplication.NameOfApplicant = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Name of agent":
                        planningApplication.NameOfAgent = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Wards":
                        planningApplication.Wards = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Parishes":
                        planningApplication.Parishes = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    case "Case officer":
                        planningApplication.CaseOfficer = dt.NextElementSibling.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;
                }
            });

            Assert.IsNotNull(planningApplication.RegisteredDate);
            Assert.IsNotNull(planningApplication.ConsultationExpiryDate);
            Assert.IsNotNull(planningApplication.TargetDate);
            Assert.IsNotNull(planningApplication.ApplicationType);
            Assert.IsNotNull(planningApplication.CurrentStatus);
            Assert.IsNotNull(planningApplication.NameOfApplicant);
            Assert.IsNotNull(planningApplication.NameOfAgent);
            Assert.IsNotNull(planningApplication.Wards);
            Assert.IsNotNull(planningApplication.Parishes);
            Assert.IsNotNull(planningApplication.CaseOfficer);

            Assert.AreNotEqual(string.Empty, planningApplication.RegisteredDate);
            Assert.AreNotEqual(string.Empty, planningApplication.ConsultationExpiryDate);
            Assert.AreNotEqual(string.Empty, planningApplication.TargetDate);
            Assert.AreNotEqual(string.Empty, planningApplication.ApplicationType);
            Assert.AreNotEqual(string.Empty, planningApplication.CurrentStatus);
            Assert.AreNotEqual(string.Empty, planningApplication.NameOfApplicant);
            Assert.AreNotEqual(string.Empty, planningApplication.NameOfAgent);
            Assert.AreNotEqual(string.Empty, planningApplication.Wards);
            Assert.AreNotEqual(string.Empty, planningApplication.Parishes);
            Assert.AreNotEqual(string.Empty, planningApplication.CaseOfficer);
        }
    }
}
