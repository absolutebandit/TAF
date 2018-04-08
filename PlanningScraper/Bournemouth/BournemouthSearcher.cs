using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Communications;
using PlanningScraper.Exceptions;
using PlanningScraper.Interfaces;
using PlanningScraper.Utils;

namespace PlanningScraper.Bournemouth
{
    public class BournemouthSearcher : ISiteSearcher
    {
        private readonly ISystemConfig _systemConfig;
        private readonly ISearchConfig _searchConfig;
        private readonly IBournemouthConfig _configuration;
        private readonly ILogger _logger;

        public BournemouthSearcher(ISystemConfig systemConfig, ISearchConfig searchConfig, IBournemouthConfig configuration, ILogger logger)
        {
            _systemConfig = systemConfig;
            _searchConfig = searchConfig;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SearchDataAndResults> ExecuteSearchAsync(string searchArea, CancellationToken cancellationToken)
        {
            try
            {
                var searchDataAndResults = new SearchDataAndResults {CookieContainer = new CookieContainer()};
                var handler = HttpClientHelpers.CreateHttpClientHandler(_systemConfig, _configuration, searchDataAndResults.CookieContainer);

                await _logger.LogInformationAsync($"Beginning searches for {searchArea.ToUpper()}...", cancellationToken);

                using (var client = new HttpClientWrapper(_configuration.BaseUri, handler, _logger, _systemConfig))
                {
                    await LogSearchInputsAsync(cancellationToken);
                    var searchPageResponse = await client.GetAsync(_configuration.SearchRoute, new CancellationToken());

                    var searchDates = await DateChunker.SplitDateRange(_searchConfig.StartDate, _searchConfig.EndDate, _configuration.ChunkSizeDays);
                    searchDataAndResults.SearchResultsPages = new List<HttpResponseMessage>();
                    client.DefaultRequestHeaders.Add("Referer", $"{_configuration.BaseUri}{_configuration.SearchRoute}");
                    searchDataAndResults.SearchResultsPages = new List<HttpResponseMessage>();

                    foreach (KeyValuePair<string, string> range in searchDates)
                    {
                        async Task<HttpRequestMessage> SearchRequestBuilder() => await BuildPostFormUrlEncodedRequestAsync(searchPageResponse, range, cancellationToken);
                        var searchPostResponse = await client.PostAsync(SearchRequestBuilder, new CancellationToken());
                        await _logger.LogInformationAsync($"Post search response status: {searchPostResponse.StatusCode}", cancellationToken);

                        var searchResults = await client.GetAsync(_configuration.SearchResultsRoute, new CancellationToken());

                        async Task<HttpRequestMessage> SearchResultsRequestBuilder() => await BuildPostAllFormUrlEncodedRequestAsync(searchResults, cancellationToken);
                        var searchPostResponseAll = await client.PostAsync(SearchResultsRequestBuilder, new CancellationToken());

                        HttpResponseMessage searchResultsAll;
                        if (searchPostResponseAll.StatusCode == HttpStatusCode.Found)
                        {
                            searchResultsAll = await client.GetAsync(_configuration.SearchResultsRoute, new CancellationToken());
                        }
                        else
                        {
                            searchResultsAll = searchPostResponseAll;
                        }

                        searchDataAndResults.SearchResultsPages.Add(searchResultsAll);
                    }

                    return searchDataAndResults;
                }
            }
            catch (Exception ex)
            {
                await _logger.LogExceptionAsync($"Search failed!", ex, cancellationToken);
                throw new SearchFailedException(ex.Message, ex.InnerException);
            }
        }

        private async Task<HttpRequestMessage> BuildPostFormUrlEncodedRequestAsync(HttpResponseMessage searchPageResponse, KeyValuePair<string, string> dateRange, CancellationToken cancellationToken)
        {
            var searchPageResponseContent = searchPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var searchPageResponseDoc = CsQuery.CQ.Create(searchPageResponseContent);
            var viewState = searchPageResponseDoc.Select("#__VIEWSTATE").Val();
            var viewStateGenerator = searchPageResponseDoc.Select("#__VIEWSTATEGENERATOR").Val();
            var eventValidation = searchPageResponseDoc.Select("#__EVENTVALIDATION").Val();

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__EVENTTARGET", string.Empty),
                new KeyValuePair<string, string>("__EVENTARGUMENT", string.Empty),
                new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
                new KeyValuePair<string, string>("ctl00$MainContent$txtAppNumber", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$RadStreetName", string.Empty),
                new KeyValuePair<string, string>("ctl00_MainContent_RadStreetName_ClientState", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtAddress", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$ddlWard", string.Empty),
                new KeyValuePair<string, string>("ctl00_MainContent_ddlWard_ClientState", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtDateReceivedFrom", dateRange.Key.Replace("-","/")),
                new KeyValuePair<string, string>("ctl00$MainContent$txtDateReceivedTo", dateRange.Value.Replace("-","/")),
                new KeyValuePair<string, string>("ctl00$MainContent$txtDateIssuedFrom", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtDateIssuedTo", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtAgentsName", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$ddlApplicationType", string.Empty),
                new KeyValuePair<string, string>("ctl00_MainContent_ddlApplicationType_ClientState", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$ddlCilChargeableCats", string.Empty),
                new KeyValuePair<string, string>("ctl00_MainContent_ddlCilChargeableCats_ClientState", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtTotCilValidFrom", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtTotCilValidTo", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtTotCilDecFrom", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtTotCilDecTo", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtTotCilCommFrom", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtTotCilCommTo", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtCilAplRecFrom", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtCilAplRecTo", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$ddlCilAplDecs", string.Empty),
                new KeyValuePair<string, string>("ctl00_MainContent_ddlCilAplDecs_ClientState", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtCilAplDecFrom", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$txtCilAplDecTo", string.Empty),
                new KeyValuePair<string, string>("ctl00$MainContent$btnSearch", "Search"),
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.SearchRoute)
            {
                Content = new FormUrlEncodedContent(keyValues)
            };

            return await Task.FromResult(request);
        }

        private async Task<HttpRequestMessage> BuildPostAllFormUrlEncodedRequestAsync(HttpResponseMessage searchResults, CancellationToken cancellationToken)
        {
            var searchResultsContent = searchResults.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var searchResutlsResponseDoc = CsQuery.CQ.Create(searchResultsContent);
            var viewState = searchResutlsResponseDoc.Select("#__VIEWSTATE").Val();
            var viewStateGenerator = searchResutlsResponseDoc.Select("#__VIEWSTATEGENERATOR").Val();
            var eventValidation = searchResutlsResponseDoc.Select("#__EVENTVALIDATION").Val();

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$MainContent$grdResults"),
                new KeyValuePair<string, string>("__EVENTARGUMENT", $"FireCommand:ctl00$MainContent$grdResults$ctl00;PageSize;{_configuration.DesiredPageSize}"),
                new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
                new KeyValuePair<string, string>("ctl00$MainContent$grdResults$ctl00$ctl03$ctl01$PageSizeComboBox", $"{_configuration.DesiredPageSize}"),
                new KeyValuePair<string, string>("ctl00_MainContent_grdResults_ctl00_ctl03_ctl01_PageSizeComboBox_ClientState", "{\"logEntries\":[],\"value\":\"" + 
                                                                                                                                $"{_configuration.DesiredPageSize}" + 
                                                                                                                                "\",\"text\":\"" + $"{_configuration.DesiredPageSize}" + 
                                                                                                                                "\",\"enabled\":true,\"checkedIndices\":[],\"checkedItemsTextOverflows\":false}"),
                new KeyValuePair<string, string>("ctl00_MainContent_grdResults_ClientState=", string.Empty),
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.SearchResultsRoute)
            {
                Content = new FormUrlEncodedContent(keyValues)
            };

            return await Task.FromResult(request);
        }

        private async Task LogSearchInputsAsync(CancellationToken cancellationToken)
        {
            var logText =
                $"{DateTime.Now} - Starting planning application search with search parameters: {Environment.NewLine}" +
                $"Search Term: {_configuration.SearchTerm}{Environment.NewLine}" +
                $"Start Date: {_searchConfig.StartDate}{Environment.NewLine}" +
                $"End Date: {_searchConfig.EndDate}{Environment.NewLine}{Environment.NewLine}";

            await _logger.LogInformationAsync(logText, cancellationToken);
        }
    }
}
