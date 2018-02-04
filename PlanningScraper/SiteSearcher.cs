using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace PlanningScraper
{
    public class SiteSearcher
    {
        private readonly string _baseUri = ConfigurationManager.AppSettings["baseUri"];
        private readonly string _keywordSearchRoute = ConfigurationManager.AppSettings["keywordSearchRoute"];
        private readonly string _defaultPageSize = ConfigurationManager.AppSettings["defaultPageSize"];
        private readonly string _desiredPageSize = ConfigurationManager.AppSettings["desiredPageSize"];
        private readonly string _searchTerm = ConfigurationManager.AppSettings["searchTerm"];
        private readonly string _dateType = ConfigurationManager.AppSettings["dateType"];
        private readonly string _startDate = ConfigurationManager.AppSettings["startDate"];
        private readonly string _endDate = ConfigurationManager.AppSettings["endDate"];

        public HttpResponseMessage ExecuteSearch(out CookieContainer cookieContainer)
        {
            try
            {
                cookieContainer = new CookieContainer();
                var handler = CreateHttpClientHandler(cookieContainer);

                using (var client = new HttpClient(handler))
                {
                    var baseAddress = new Uri(_baseUri);
                    client.BaseAddress = baseAddress;

                    var searchPageResponse = client.GetAsync(_keywordSearchRoute).GetAwaiter().GetResult();
                    var request = BuildPostFormUrlEncodedRequest(searchPageResponse);
                    client.DefaultRequestHeaders.Add("Referer", $"{_baseUri}{_keywordSearchRoute}");

                    var searchPostResponse = client.SendAsync(request).GetAwaiter().GetResult();
                    var redirectUrl = searchPostResponse.Headers.Location.ToString().Replace($"PS={_defaultPageSize}", $"PS={_desiredPageSize}");
                    var searchResults = client.GetAsync(redirectUrl, CancellationToken.None).GetAwaiter().GetResult();

                    return searchResults;
                }
            }
            catch (Exception ex)
            {
                throw new SearchFailedException(ex.Message, ex.InnerException);
            }
        }

        public static HttpClientHandler CreateHttpClientHandler(CookieContainer cookieContainer)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false
            };
            return handler;
        }

        private HttpRequestMessage BuildPostFormUrlEncodedRequest(HttpResponseMessage searchPageResponse)
        {
            var searchPageResponseContent = searchPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var searchPageResponseDoc = CsQuery.CQ.Create(searchPageResponseContent);
            var viewState = searchPageResponseDoc.Select("#__VIEWSTATE").Val();
            var viewStateGenerator = searchPageResponseDoc.Select("#__VIEWSTATEGENERATOR").Val();

            var keyValues = new List<KeyValuePair<string, string>>();
            keyValues.Add(new KeyValuePair<string, string>("__VIEWSTATE", viewState));
            keyValues.Add(new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator));
            keyValues.Add(new KeyValuePair<string, string>("txtProposal", _searchTerm));
            keyValues.Add(new KeyValuePair<string, string>("rbGroup", "rbRange"));
            keyValues.Add(new KeyValuePair<string, string>("cboSelectDateValue", _dateType));
            keyValues.Add(new KeyValuePair<string, string>("cboDays", "1"));
            keyValues.Add(new KeyValuePair<string, string>("cboMonths", "1"));
            keyValues.Add(new KeyValuePair<string, string>("dateStart", _startDate));
            keyValues.Add(new KeyValuePair<string, string>("dateEnd", _endDate));
            keyValues.Add(new KeyValuePair<string, string>("edrDateSelection", ""));
            keyValues.Add(new KeyValuePair<string, string>("csbtnSearch", "Search"));

            var request = new HttpRequestMessage(HttpMethod.Post, "/Northgate/PlanningExplorer/KeywordsSearch.aspx");
            request.Content = new FormUrlEncodedContent(keyValues);
            return request;
        }

    }
}
