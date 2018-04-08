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

namespace PlanningScraper.Bournemouth
{
    public class BournemouthExtractor : IPlanningDataExtractor
    {
        private readonly List<PlanningApplication> _planningApplications = new List<PlanningApplication>();
        private readonly IBournemouthConfig _configuration;
        private readonly ISystemConfig _systemConfig;
        private readonly ILogger _logger;

        public BournemouthExtractor(ISystemConfig systemConfig, IBournemouthConfig configuration, ILogger logger)
        {
            _systemConfig = systemConfig;
            _configuration = configuration;
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

                    var row = 0;
                    searchPageResponseDoc.Select("table tbody tr").Each(searchRow =>
                    {
                        row++;

                        // Filter by proposal text containing search term.
                        if (searchRow.ChildNodes[4].InnerText.Contains(_configuration.SearchTerm))
                        {
                            var planningApplication = new PlanningApplication();

                            var appLink = searchRow.ChildNodes[2].ChildNodes[1].Attributes["href"];
                            _logger.LogInformationAsync($"Getting application detail for result number {row} application {appLink}", cancellationToken).GetAwaiter().GetResult();

                            var appDetailsResponse = client.GetAsync(appLink, cancellationToken).GetAwaiter().GetResult();
                            var content = appDetailsResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                            var appDetailsResponseDoc = CQ.Create(content);

                            appDetailsResponseDoc.Select("#MainContent_RadPageView1 .form tr").Each(tr =>
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

                            appDetailsResponseDoc.Select("#MainContent_RadPageView2 .form tr").Each(tr =>
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

                            _planningApplications.Add(planningApplication);

                            if (_configuration.UseProxy)
                            {
                                // refresh client/handler to get a new IP address
                                client = HttpClientHelpers.CreateClient(_configuration.BaseUri, _systemConfig, _configuration, _logger, cookieContainer);
                            }
                        }
                    });
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
                        planningApplication.AgentName = dt.NextElementSibling.InnerText.Clean();
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
