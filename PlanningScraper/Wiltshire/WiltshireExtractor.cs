using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using PlanningScraper.Communications;
using PlanningScraper.Exceptions;
using PlanningScraper.Extensions;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Wiltshire
{
    public class WiltshireExtractor : IPlanningDataExtractor
    {
        private readonly List<PlanningApplication> _planningApplications = new List<PlanningApplication>();
        private readonly IWiltshireConfig _configuration;
        private readonly ISystemConfig _systemConfig;
        private readonly ILogger _logger;

        public WiltshireExtractor(ISystemConfig systemConfig, IWiltshireConfig configuration, ILogger logger)
        {
            _systemConfig = systemConfig;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<IEnumerable<PlanningApplication>> ExtractDataAsync(List<HttpResponseMessage> searchResultPages, CookieContainer cookieContainer, CancellationToken cancellationToken)
        {
            try
            {
                var searchResults = searchResultPages.First();
                var client = HttpClientHelpers.CreateClient(_systemConfig, _configuration, _logger, cookieContainer);
                var baseAddress = new Uri(_configuration.BaseUri);
                client.BaseAddress = baseAddress;
                client.DefaultRequestHeaders.Add("Referer", $"{searchResults.RequestMessage.RequestUri}");

                var searchResultsHtml = searchResults.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var searchPageResponseDoc = CQ.Create(searchResultsHtml);
                    
                await _logger.LogInformationAsync($"Found {searchPageResponseDoc.Select("table tbody tr").Length} planning applications...", cancellationToken);

                var row = 1;
                searchPageResponseDoc.Select("table tbody tr").Each(tr =>
                {

                    var planningApplication = new PlanningApplication();
                    GetSeachRowDetailAsync(tr, planningApplication, cancellationToken).GetAwaiter().GetResult();

                    _logger.LogInformationAsync($"Getting application detail for result number {row} application reference {planningApplication.ApplicationReference} from {Environment.NewLine}{planningApplication.ApplicationLink}", cancellationToken).GetAwaiter().GetResult(); 

                    GetPlanningApplicationDetailAsync(client, planningApplication, cancellationToken).GetAwaiter().GetResult();

                    GetPlanningApplicationDocumentLinksAsync(client, planningApplication, cancellationToken).GetAwaiter().GetResult();

                    _planningApplications.Add(planningApplication);

                    // refresh client/handler to get a new IP address
                    client = HttpClientHelpers.CreateClient(_systemConfig, _configuration, _logger, cookieContainer);
                    row++;
                });

                await _logger.LogInformationAsync($"Finished extracting planning data...", cancellationToken);

                client.Dispose();

                return await Task.FromResult(_planningApplications);
            }
            catch (Exception ex)
            {
                throw new ExtractDataFailedException(ex.Message, ex.InnerException);
            }
        }

        public static async Task GetSeachRowDetailAsync(IDomObject tr, PlanningApplication planningApplication, CancellationToken cancellationToken)
        {
            var cellPos = 0;
            foreach (var cell in tr.ChildElements)
            {
                switch (cellPos)
                {
                    // Application
                    case 0:
                        planningApplication.ApplicationReference = cell.InnerText.Clean();
                        planningApplication.ApplicationLink = cell.FirstElementChild.Attributes["href"].CleanApplicationUrl();
                        break;

                    // Site
                    case 1:
                        planningApplication.SiteAddress = cell.InnerText.Trim().Clean();
                        break;

                    // Proposal
                    case 2:
                        planningApplication.Proposal = cell.InnerText.Trim().Clean();
                        break;
                }
                cellPos++;
            }

            await Task.CompletedTask;
        }

        public async Task GetPlanningApplicationDetailAsync(HttpClientWrapper client, PlanningApplication planningApplication, CancellationToken cancellationToken)
        {
            var applicationPageResponse = await client.GetAsync($"{_configuration.ApplicationPageRoute}{planningApplication.ApplicationLink}", new CancellationToken());
            var applicationHtml = applicationPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var applicationPageDoc = CsQuery.CQ.Create(applicationHtml);

            await ExtractPlanningApplicationData(planningApplication, applicationPageDoc);
        }

        public static async Task ExtractPlanningApplicationData(PlanningApplication planningApplication, CQ applicationPageDoc)
        {
            applicationPageDoc.Select(".planappkd dl dt").Each(dt =>
            {
                switch (dt.InnerText)
                {
                    case "Registered (validated)":
                        planningApplication.RegisteredDate = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Consultation expiry":
                        planningApplication.ConsultationExpiryDate = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Target date for decision":
                        planningApplication.TargetDate = dt.NextElementSibling.InnerText.Clean();
                        break;
                }
            });

            applicationPageDoc.Select("#wrapper dl:nth-of-type(2) dt").Each(dt =>
            {
                switch (dt.InnerText)
                {
                    case "Application type":
                        planningApplication.ApplicationType = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Current status of application":
                        planningApplication.CurrentStatus = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Name of applicant":
                        planningApplication.NameOfApplicant = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Name of agent":
                        planningApplication.NameOfAgent = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Wards":
                        planningApplication.Wards = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Parishes":
                        planningApplication.Parishes = dt.NextElementSibling.InnerText.Clean();
                        break;

                    case "Case officer":
                        planningApplication.CaseOfficer = dt.NextElementSibling.InnerText.Clean();
                        break;
                }
            });

            planningApplication.DocumentsLink = applicationPageDoc.Select(".documentlink a").Attr("href");

            await Task.CompletedTask;
        }

        public async Task GetPlanningApplicationDocumentLinksAsync(HttpClientWrapper client, PlanningApplication planningApplication, CancellationToken cancellationToken)
        {
            var documentSearchResponse = await client.GetAsync($"{planningApplication.DocumentsLink}", new CancellationToken());
            var documentSearchHtml = await documentSearchResponse.Content.ReadAsStringAsync();
            var documentSeachDoc = CQ.Create(documentSearchHtml);

            await ExtractDocumentLinksAsync(planningApplication, documentSeachDoc, cancellationToken);
        }

        public static async Task ExtractDocumentLinksAsync(PlanningApplication planningApplication, CQ documentSeachDoc, CancellationToken cancellationToken)
        {
            documentSeachDoc.Select(".multiFilePanelContainer").Each(fileContainer =>
            {
                var fileDiv = CQ.Create(fileContainer);
                var docTitle = fileDiv.Select(".filePanel div").Attr("title").Clean();
                var docLink = fileDiv.Select("a").Attr("href").Clean();
                planningApplication.AllDocumentLinks = planningApplication.AllDocumentLinks +
                                                       $"{docTitle} - http://unidoc.wiltshire.gov.uk{docLink} ";
            });

            await Task.CompletedTask;
        }
    }
}
