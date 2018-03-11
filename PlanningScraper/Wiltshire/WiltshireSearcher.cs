using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Communications;
using PlanningScraper.Exceptions;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Wiltshire
{
    public class WiltshireSearcher : ISiteSearcher
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        public WiltshireSearcher(IConfiguration configuration, ILogger logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<SearchDataAndResults> ExecuteSearchAsync(CancellationToken cancellationToken)
        {
            try
            {
                var searchDataAndResults = new SearchDataAndResults {CookieContainer = new CookieContainer()};
                var handler = HttpClientHelpers.CreateHttpClientHandler(_configuration, searchDataAndResults.CookieContainer);

                using (var client = new HttpClientWrapper(handler, _logger))
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
                    await Task.Delay(_configuration.RequestDelayTimeSpan, cancellationToken);

                    var searchPostResponse = await client.SendAsync(request, cancellationToken);
                    var redirectUrl = searchPostResponse.Headers.Location.ToString()
                        .Replace($"PS={_configuration.DefaultPageSize}", $"PS={_configuration.DesiredPageSize}");
                    await Task.Delay(_configuration.RequestDelayTimeSpan, cancellationToken);

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
                new KeyValuePair<string, string>("txtProposal", _configuration.SearchTerm),
                new KeyValuePair<string, string>("rbGroup", "rbRange"),
                new KeyValuePair<string, string>("cboSelectDateValue", _configuration.DateType),
                new KeyValuePair<string, string>("cboDays", "1"),
                new KeyValuePair<string, string>("cboMonths", "1"),
                new KeyValuePair<string, string>("dateStart", _configuration.StartDate),
                new KeyValuePair<string, string>("dateEnd", _configuration.EndDate),
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
                $"Search Term: {_configuration.SearchTerm}{Environment.NewLine}" +
                $"Date Type: {_configuration.DateType}{Environment.NewLine}" +
                $"Start Date: {_configuration.StartDate}{Environment.NewLine}" +
                $"End Date: {_configuration.EndDate}{Environment.NewLine}{Environment.NewLine}";

            await _logger.LogInformationAsync(logText, cancellationToken);
        }
    }
}
