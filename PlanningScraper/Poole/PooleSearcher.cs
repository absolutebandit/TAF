using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Communications;
using PlanningScraper.Exceptions;
using PlanningScraper.Interfaces;

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

                    LogSearchInputsAsync(cancellationToken);

                    var request = new HttpRequestMessage();
                    request.Build(this._configuration, HttpMethod.Get, _configuration.KeywordSearchRoute);
                    var searchPageResponse = await client.SendAsync(request, new CancellationToken());

                    request = await BuildPostFormUrlEncodedRequestAsync(searchPageResponse, cancellationToken);
                    client.DefaultRequestHeaders.Add("Referer",
                        $"{_configuration.BaseUri}{_configuration.KeywordSearchRoute}");

                    var searchPostResponse = await client.SendAsync(request, cancellationToken);
                    var redirectUrl = searchPostResponse.Headers.Location.ToString()
                        .Replace($"PS={_configuration.DefaultPageSize}", $"PS={_configuration.DesiredPageSize}");

                    request = new HttpRequestMessage().Build(_configuration, HttpMethod.Get, redirectUrl);
                    var searchResults = await client.SendAsync(request, new CancellationToken());

                    await _logger.LogInformationAsync($"Post search response status: {searchResults.StatusCode}", cancellationToken);

                    searchDataAndResults.SearchResultsPages = new List<HttpResponseMessage> { searchResults };
                    return searchDataAndResults;
                }
            }
            catch (Exception ex)
            {
                await _logger.LogExceptionAsync($"Search failed!", ex, cancellationToken);
                throw new SearchFailedException(ex.Message, ex.InnerException);
            }
        }

        private async Task<HttpRequestMessage> BuildPostFormUrlEncodedRequestAsync(HttpResponseMessage searchPageResponse, CancellationToken cancellationToken)
        {
            var searchPageResponseContent = searchPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var searchPageResponseDoc = CsQuery.CQ.Create(searchPageResponseContent);
            var viewState = searchPageResponseDoc.Select("#__VIEWSTATE").Val();
            var viewStateGenerator = searchPageResponseDoc.Select("#__VIEWSTATEGENERATOR").Val();

            var keyValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("__VIEWSTATE", viewState),
                new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator),
                new KeyValuePair<string, string>("txtProposal", _searchConfig.SearchTerm),
                new KeyValuePair<string, string>("rbGroup", "rbRange"),
                new KeyValuePair<string, string>("cboSelectDateValue", _searchConfig.DateType),
                new KeyValuePair<string, string>("cboDays", "1"),
                new KeyValuePair<string, string>("cboMonths", "1"),
                new KeyValuePair<string, string>("dateStart", _searchConfig.StartDate),
                new KeyValuePair<string, string>("dateEnd", _searchConfig.EndDate),
                new KeyValuePair<string, string>("edrDateSelection", ""),
                new KeyValuePair<string, string>("csbtnSearch", "Search")
            };

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration.KeywordSearchRoute)
            {
                Content = new FormUrlEncodedContent(keyValues)
            };

            return await Task.FromResult(request);
        }

        private async void LogSearchInputsAsync(CancellationToken cancellationToken)
        {
            var logText =
                $"{DateTime.Now} - Starting planning application search with search parameters: {Environment.NewLine}" +
                $"Search Term: {_searchConfig.SearchTerm}{Environment.NewLine}" +
                $"Date Type: {_searchConfig.DateType}{Environment.NewLine}" +
                $"Start Date: {_searchConfig.StartDate}{Environment.NewLine}" +
                $"End Date: {_searchConfig.EndDate}{Environment.NewLine}{Environment.NewLine}";

            await _logger.LogInformationAsync(logText, cancellationToken);
        }
    }
}
