using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using CsQuery;

namespace PlanningScraper
{
    public class PlanningDataExtractor
    {
        private readonly string _baseUri = ConfigurationManager.AppSettings["baseUri"];
        private readonly List<PlanningApplication> _planningApplications = new List<PlanningApplication>();
        private static readonly string ApplicationPageRoute = ConfigurationManager.AppSettings["applicationPageRoute"];
        private static readonly string LogFile = ConfigurationManager.AppSettings["logFileLocation"];

        public IEnumerable<PlanningApplication> ExtractData(HttpResponseMessage searchResults, CookieContainer cookieContainer)
        {
            try
            {
                var handler = SiteSearcher.CreateHttpClientHandler(cookieContainer);

                using (var client = new HttpClient(handler))
                {
                    var baseAddress = new Uri(_baseUri);
                    client.BaseAddress = baseAddress;
                    client.DefaultRequestHeaders.Add("Referer", $"{searchResults.RequestMessage.RequestUri}");

                    var searchResultsHtml = searchResults.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var searchPageResponseDoc = CQ.Create(searchResultsHtml);
                    
                    LogSearchResults(searchResultsHtml, searchPageResponseDoc);

                    var row = 1;
                    searchPageResponseDoc.Select("table tbody tr").Each(tr =>
                    {
                        var planningApplication = new PlanningApplication();
                        GetSeachRowDetail(tr, planningApplication);

                        LogApplicationRetrieval(row, planningApplication);

                        GetPlanningApplicationDetail(client, planningApplication);

                        _planningApplications.Add(planningApplication);

                        row++;
                    });

                    LogFinishedExtract();

                    return _planningApplications;
                }
            }
            catch (Exception ex)
            {
                throw new ExtractDataFailedException(ex.Message, ex.InnerException);
            }
        }

        private static void GetSeachRowDetail(IDomObject tr, PlanningApplication planningApplication)
        {
            var cellPos = 0;
            foreach (var cell in tr.ChildElements)
            {
                switch (cellPos)
                {
                    // Application
                    case 0:
                        planningApplication.ApplicationReference = cell.InnerText.Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        planningApplication.ApplicationLink = cell.FirstElementChild.Attributes["href"].Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;

                    // Site
                    case 1:
                        planningApplication.SiteAddress = cell.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", "; ");
                        break;

                    // Proposal
                    case 2:
                        planningApplication.Proposal = cell.InnerText.Trim().Replace("\t", string.Empty).Replace("\r\n", string.Empty).Replace("\n", string.Empty);
                        break;
                }
                cellPos++;
            }
        }

        private static void GetPlanningApplicationDetail(HttpClient client, PlanningApplication planningApplication)
        {
            var applicationPageResponse = client.GetAsync($"{ApplicationPageRoute}{planningApplication.ApplicationLink}").GetAwaiter().GetResult();
            var applicationHtml = applicationPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var applicationPageDoc = CsQuery.CQ.Create(applicationHtml);

            applicationPageDoc.Select(".planappkd dl dt").Each(dt =>
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

            applicationPageDoc.Select("#wrapper dl:nth-of-type(2) dt").Each(dt =>
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
        }

        private static void LogFinishedExtract()
        {
            var logText = $"Finished extracting planning data...{Environment.NewLine}{Environment.NewLine}";
            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }

        private static void LogSearchResults(string searchResultsHtml, CQ searchPageResponseDoc)
        {
            Console.WriteLine($"Found {searchPageResponseDoc.Select("table tbody tr").Length} planning applications...{Environment.NewLine}{Environment.NewLine}");
            var logText = $"Search Results HTML: {Environment.NewLine}{searchResultsHtml}{Environment.NewLine}{Environment.NewLine}";
            File.AppendAllText(LogFile, logText);
        }

        private static void LogApplicationRetrieval(int row, PlanningApplication planningApplication)
        {
            var logText = $"Getting application detail for result number {row} application reference {planningApplication.ApplicationReference} from {Environment.NewLine}{planningApplication.ApplicationLink}{Environment.NewLine}{Environment.NewLine}";
            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }
    }
}
