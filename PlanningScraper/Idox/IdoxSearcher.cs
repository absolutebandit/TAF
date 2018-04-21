using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CsQuery;
using PlanningScraper.Communications;
using PlanningScraper.Configuration;
using PlanningScraper.Exceptions;
using PlanningScraper.Interfaces;
using PlanningScraper.Utils;

namespace PlanningScraper.Idox
{
    public class IdoxSearcher : ISiteSearcher
    {
        private readonly ISystemConfig _systemConfig;
        private readonly ISearchConfig _searchConfig;
        private readonly ILogger _logger;
        private readonly INamedInstanceResolver<IIdoxConfig> _configResolver;
        private IIdoxConfig _configuration;

        public IdoxSearcher(INamedInstanceResolver<IIdoxConfig> configResolver, ISystemConfig systemConfig, ISearchConfig searchConfig, ILogger logger)
        {
            _systemConfig = systemConfig;
            _searchConfig = searchConfig;
            _configResolver = configResolver;
            _logger = logger;
        }

        public async Task<SearchDataAndResults> ExecuteSearchAsync(string searchArea, CancellationToken cancellationToken)
        {
            try
            {
                _configuration = _configResolver.ResolveConfig(searchArea);

                var searchDataAndResults = new SearchDataAndResults {CookieContainer = new CookieContainer()};
                var handler = HttpClientHelpers.CreateHttpClientHandler(_systemConfig, _configuration, searchDataAndResults.CookieContainer);

                await _logger.LogInformationAsync($"Beginning searches for {searchArea.ToUpper()}...", cancellationToken);

                using (var client = new HttpClientWrapper(_configuration.BaseUri, handler, _logger, _systemConfig))
                {
                    await LogSearchInputsAsync(cancellationToken);

                    await client.GetAsync(_configuration.SearchRoute, new CancellationToken());

                    var searchDates = await DateChunker.SplitDateRange(_searchConfig.StartDate, _searchConfig.EndDate, _configuration.ChunkSizeDays);
                    searchDataAndResults.SearchResultsPages = new List<HttpResponseMessage>();

                    foreach (KeyValuePair<string, string> range in searchDates)
                    {
                        client.DefaultRequestHeaders.Remove("Referer");
                        client.DefaultRequestHeaders.Add("Referer", $"{_configuration.BaseUri}{_configuration.SearchRoute}");

                        async Task<HttpRequestMessage> PostRequestBuilder() => await BuildPostFormUrlEncodedRequestAsync(range);
                        var initialSearchResponse = await client.PostAsync(PostRequestBuilder, cancellationToken);

                        if (!await SearchResultsExist(initialSearchResponse))
                        {
                            continue;
                        }

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

        private async Task<bool> SearchResultsExist(HttpResponseMessage initialSearchResponse)
        {
            var initialSearchResponseHtml = await initialSearchResponse.Content.ReadAsStringAsync();
            var initialSearchPageResponseDoc = CQ.Create(initialSearchResponseHtml);
            var searchResultList = initialSearchPageResponseDoc.Select("#searchresults");
            return searchResultList.Length != 0;
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
