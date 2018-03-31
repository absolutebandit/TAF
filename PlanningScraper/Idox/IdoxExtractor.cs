using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using PlanningScraper.Communications;
using PlanningScraper.Exceptions;
using PlanningScraper.Extensions;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Idox
{
    public class IdoxExtractor : IPlanningDataExtractor
    {
        private readonly List<PlanningApplication> _planningApplications = new List<PlanningApplication>();
        private readonly IIdoxConfig _configuration;
        private readonly ISystemConfig _systemConfig;
        private readonly ILogger _logger;

        public IdoxExtractor(ISystemConfig systemConfig, IIdoxConfig configuration, ILogger logger)
        {
            _configuration = configuration;
            _systemConfig = systemConfig;
            _logger = logger;
        }

        public async Task<IEnumerable<PlanningApplication>> ExtractDataAsync(string searchArea, List<HttpResponseMessage> searchResultPages, CookieContainer cookieContainer, CancellationToken cancellationToken)
        {
            try
            {
                var currentPage = 0;

                await _logger.LogInformationAsync($"Processing {searchResultPages.Count} search result pages for {searchArea.ToUpper()}", cancellationToken);

                var client = HttpClientHelpers.CreateClient(_configuration.BaseUri, _systemConfig, _configuration, _logger, cookieContainer);

                foreach (var searchResults in searchResultPages)
                {
                    currentPage++;
                    var searchResultsHtml = await searchResults.Content.ReadAsStringAsync();
                    var searchPageResponseDoc = CQ.Create(searchResultsHtml);

                    var applicationPathLinks = searchPageResponseDoc.Select(".searchresult a");

                    await _logger.LogInformationAsync($"Found {applicationPathLinks.Length} planning applications in page {currentPage}...", cancellationToken);

                    var row = 0;
                    foreach (IDomObject appPathLink in applicationPathLinks)
                    {
                        row++;
                        var planningApplication = new PlanningApplication();
                        var appSummaryPath = appPathLink.Attributes["href"];
                        await _logger.LogInformationAsync($"Getting application detail for result number {row} application {appSummaryPath}", cancellationToken);

                        await ExtractApplicationSummary(cancellationToken, appSummaryPath, client, planningApplication);

                        var appDetailsPath = await ExtractApplicationDetails(cancellationToken, appSummaryPath, client, planningApplication);

                        await ExtractApplicationContact(cancellationToken, appDetailsPath, client, planningApplication);

                        _planningApplications.Add(planningApplication);

                        if (_configuration.UseProxy)
                        {
                            // refresh client/handler to get a new IP address
                            client = HttpClientHelpers.CreateClient(_configuration.BaseUri, _systemConfig, _configuration, _logger, cookieContainer);
                        }
                    }
                }

                await _logger.LogInformationAsync($"Finished extracting planning data for {searchArea.ToUpper()}...", cancellationToken);

                client.Dispose();

                return _planningApplications;
            }
            catch (Exception ex)
            {
                throw new ExtractDataFailedException(ex.Message, ex.InnerException);
            }
        }

        private static async Task ExtractApplicationContact(CancellationToken cancellationToken, string appDetailsPath,
            HttpClientWrapper client, PlanningApplication planningApplication)
        {
            var appContactsPath = appDetailsPath.Replace("activeTab=details", "activeTab=contacts");
            var applicationContactsResponse = await client.GetAsync(appContactsPath, cancellationToken);
            var applicationContactsResponseDoc = CQ.Create(await applicationContactsResponse.Content.ReadAsStringAsync());

            applicationContactsResponseDoc.Select(".agents tr").Each(row =>
            {
                if (row.ChildNodes[1].InnerText.Contains("Personal Email"))
                {
                    planningApplication.AgentEmail = row.ChildNodes[3].InnerText.Clean();
                }
            });
        }

        private static async Task<string> ExtractApplicationDetails(CancellationToken cancellationToken, string appSummaryPath,
            HttpClientWrapper client, PlanningApplication planningApplication)
        {
            var appDetailsPath = appSummaryPath.Replace("activeTab=summary", "activeTab=details");
            var applicationDetailsResponse = await client.GetAsync(appDetailsPath, cancellationToken);
            var applicationDetailsResponseDoc = CQ.Create(await applicationDetailsResponse.Content.ReadAsStringAsync());

            applicationDetailsResponseDoc.Select("#applicationDetails tr").Each(row =>
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
            return appDetailsPath;
        }

        private static async Task ExtractApplicationSummary(CancellationToken cancellationToken, string appSummaryPath,
            HttpClientWrapper client, PlanningApplication planningApplication)
        {
            var applicationSummaryResponse = await client.GetAsync(appSummaryPath, cancellationToken);
            var applicationSummaryResponseDoc = CQ.Create(await applicationSummaryResponse.Content.ReadAsStringAsync());

            applicationSummaryResponseDoc.Select("#simpleDetailsTable tr").Each(row =>
            {
                if (row.ChildNodes[1].InnerText.Contains("Reference"))
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

                if (row.ChildNodes[1].InnerText.Contains("Status"))
                {
                    planningApplication.CurrentStatus = row.ChildNodes[3].InnerText.Clean();
                }
            });
        }
    }
}
