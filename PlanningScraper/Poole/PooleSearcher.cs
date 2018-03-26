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

namespace PlanningScraper.Poole
{
    public class PooleSearcher : ISiteSearcher
    {
        private readonly ISystemConfig _systemConfig;
        private readonly ISearchConfig _searchConfig;
        private readonly IPooleConfig _configuration;
        private readonly ILogger _logger;

        public PooleSearcher(ISystemConfig systemConfig, ISearchConfig searchConfig, IPooleConfig configuration, ILogger logger)
        {
            _systemConfig = systemConfig;
            _searchConfig = searchConfig;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SearchDataAndResults> ExecuteSearchAsync(CancellationToken cancellationToken)
        {
            try
            {
                var searchDataAndResults = new SearchDataAndResults {CookieContainer = new CookieContainer()};
                var handler = HttpClientHelpers.CreateHttpClientHandler(_systemConfig, _configuration, searchDataAndResults.CookieContainer);

                using (var client = new HttpClientWrapper(handler, _logger, _systemConfig))
                {
                    var baseAddress = new Uri(_configuration.BaseUri);
                    client.BaseAddress = baseAddress;

                    await LogSearchInputsAsync(cancellationToken);

                    await client.GetAsync(_configuration.searchRoute, new CancellationToken());

                    var searchDates = await DateChunker.SplitDateRange(_searchConfig.StartDate, _searchConfig.EndDate, _configuration.ChunkSizeDays);
                    searchDataAndResults.SearchResultsPages = new List<HttpResponseMessage>();

                    foreach (KeyValuePair<string, string> range in searchDates)
                    {
                        client.DefaultRequestHeaders.Remove("Referer");
                        client.DefaultRequestHeaders.Add("Referer", $"{_configuration.BaseUri}{_configuration.searchRoute}");

                        async Task<HttpRequestMessage> PostRequestBuilder() => await BuildPostFormUrlEncodedRequestAsync(range);
                        await client.PostAsync(PostRequestBuilder, cancellationToken);

                        client.DefaultRequestHeaders.Remove("Referer");
                        client.DefaultRequestHeaders.Add("Referer", $"{_configuration.BaseUri}{_configuration.AdvancedSearchRoute}");

                        async Task<HttpRequestMessage> PagedRequestBuilder() => await BuildPagedSearchResultsRequestAsync();
                        var searchResults = await client.PostAsync(PagedRequestBuilder, new CancellationToken());

                        await _logger.LogInformationAsync($"Post search response status: {searchResults.StatusCode}", cancellationToken);
                        searchDataAndResults.SearchResultsPages.Add(searchResults);
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

        private async Task<HttpRequestMessage> BuildPostFormUrlEncodedRequestAsync(KeyValuePair<string, string> range)
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("searchCriteria.reference", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.planningPortalReference", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.alternativeReference", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.description", _configuration.SearchTerm),
                new KeyValuePair<string, string>("searchCriteria.applicantName", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.caseType", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.ward", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.conservationArea", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.caseStatus", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.caseDecision", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.appealStatus", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.appealDecision", string.Empty),
                new KeyValuePair<string, string>("searchCriteria.developmentType", string.Empty),
                new KeyValuePair<string, string>("caseAddressType", "Application"),
                new KeyValuePair<string, string>("searchCriteria.address", string.Empty),
                new KeyValuePair<string, string>("date(applicationValidatedStart)", range.Key.Replace("-","/")),
                new KeyValuePair<string, string>("date(applicationValidatedEnd)", range.Value.Replace("-","/")),
                new KeyValuePair<string, string>("date(applicationCommitteeStart)", string.Empty),
                new KeyValuePair<string, string>("date(applicationCommitteeEnd)", string.Empty),
                new KeyValuePair<string, string>("date(applicationDecisionStart)", string.Empty),
                new KeyValuePair<string, string>("date(applicationDecisionEnd)", string.Empty),
                new KeyValuePair<string, string>("date(appealDecisionStart)", string.Empty),
                new KeyValuePair<string, string>("date(appealDecisionEnd)", string.Empty),
                new KeyValuePair<string, string>("searchType", "Application")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.AdvancedSearchRoute)
            {
                Content = new FormUrlEncodedContent(keyValues)
            };

            return await Task.FromResult(request);
        }

        private async Task<HttpRequestMessage> BuildPagedSearchResultsRequestAsync()
        {
            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("searchCriteria.page", "1"),
                new KeyValuePair<string, string>("action", "page"),
                new KeyValuePair<string, string>("orderBy", "DateReceived"),
                new KeyValuePair<string, string>("orderByDirection", "Descending"),
                new KeyValuePair<string, string>("searchCriteria.resultsPerPage", "100000"),
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.PagedSearchResultsRoute)
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
