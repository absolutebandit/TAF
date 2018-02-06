using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace PlanningScraper
{
    public class SiteSearcher
    {
        private static readonly string BaseUri = ConfigurationManager.AppSettings["baseUri"];
        private static readonly string KeywordSearchRoute = ConfigurationManager.AppSettings["keywordSearchRoute"];
        private static readonly string DefaultPageSize = ConfigurationManager.AppSettings["defaultPageSize"];
        private static readonly string DesiredPageSize = ConfigurationManager.AppSettings["desiredPageSize"];
        private static readonly string SearchTerm = ConfigurationManager.AppSettings["searchTerm"];
        private static readonly string DateType = ConfigurationManager.AppSettings["dateType"];
        private static readonly string StartDate = ConfigurationManager.AppSettings["startDate"];
        private static readonly string EndDate = ConfigurationManager.AppSettings["endDate"];
        private static readonly string LogFile = ConfigurationManager.AppSettings["logFileLocation"];

        public HttpResponseMessage ExecuteSearch(out CookieContainer cookieContainer)
        {
            try
            {
                cookieContainer = new CookieContainer();
                var handler = CreateHttpClientHandler(cookieContainer);

                using (var client = new HttpClient(handler))
                {
                    var baseAddress = new Uri(BaseUri);
                    client.BaseAddress = baseAddress;

                    LogSearchInputs();

                    var searchPageResponse = client.GetAsync(KeywordSearchRoute).GetAwaiter().GetResult();
                    var request = BuildPostFormUrlEncodedRequest(searchPageResponse);
                    client.DefaultRequestHeaders.Add("Referer", $"{BaseUri}{KeywordSearchRoute}");

                    var searchPostResponse = client.SendAsync(request).GetAwaiter().GetResult();
                    var redirectUrl = searchPostResponse.Headers.Location.ToString().Replace($"PS={DefaultPageSize}", $"PS={DesiredPageSize}");
                    var searchResults = client.GetAsync(redirectUrl, CancellationToken.None).GetAwaiter().GetResult();

                    LogSearchOutputs(searchResults);

                    return searchResults;
                }
            }
            catch (Exception ex)
            {
                LogSearchError(ex);
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
            keyValues.Add(new KeyValuePair<string, string>("txtProposal", SearchTerm));
            keyValues.Add(new KeyValuePair<string, string>("rbGroup", "rbRange"));
            keyValues.Add(new KeyValuePair<string, string>("cboSelectDateValue", DateType));
            keyValues.Add(new KeyValuePair<string, string>("cboDays", "1"));
            keyValues.Add(new KeyValuePair<string, string>("cboMonths", "1"));
            keyValues.Add(new KeyValuePair<string, string>("dateStart", StartDate));
            keyValues.Add(new KeyValuePair<string, string>("dateEnd", EndDate));
            keyValues.Add(new KeyValuePair<string, string>("edrDateSelection", ""));
            keyValues.Add(new KeyValuePair<string, string>("csbtnSearch", "Search"));

            var request = new HttpRequestMessage(HttpMethod.Post, "/Northgate/PlanningExplorer/KeywordsSearch.aspx");
            request.Content = new FormUrlEncodedContent(keyValues);
            return request;
        }

        private void LogSearchInputs()
        {
            var logText = $"{DateTime.Now} - Starting planning application search with search parameters: {Environment.NewLine}" +
                          $"Search Term: {SearchTerm}{Environment.NewLine}" +
                          $"Date Type: {DateType}{Environment.NewLine}" +
                          $"Start Date: {StartDate}{Environment.NewLine}" +
                          $"End Date: {EndDate}{Environment.NewLine}{Environment.NewLine}";

            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }

        private void LogSearchOutputs(HttpResponseMessage searchResults)
        {
            var logText = $"Post search response status: {searchResults.StatusCode}{Environment.NewLine}{Environment.NewLine}";

            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }

        private void LogSearchError(Exception ex)
        {
            var logText = $"Search failed! {Environment.NewLine}{ex}{Environment.NewLine}{Environment.NewLine}";
            Console.WriteLine(logText);
            File.AppendAllText(LogFile, logText);
        }
    }
}
