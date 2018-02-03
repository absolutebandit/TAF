using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningScraper
{
    class Program
    {
        static void Main(string[] args)
        {
            var cookieContainer = new CookieContainer();

            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer
            };

            using (var client = new HttpClient(handler))
            {
                var baseAddress = new Uri("http://planning.wiltshire.gov.uk");
                client.BaseAddress = baseAddress;
                
                var searchPageResponse = client.GetAsync("/Northgate/PlanningExplorer/KeywordsSearch.aspx").GetAwaiter().GetResult();
                var searchPageResponseContent = searchPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                SetDefaultHeaders(client);

                var searchPageResponseDoc = CsQuery.CQ.Create(searchPageResponseContent);
                var viewState = searchPageResponseDoc.Select("#__VIEWSTATE").Val();
                var viewStateGenerator = searchPageResponseDoc.Select("#__VIEWSTATEGENERATOR").Val();
                
                var keyValues = new List<KeyValuePair<string, string>>();
                keyValues.Add(new KeyValuePair<string, string>("__VIEWSTATE", viewState));
                keyValues.Add(new KeyValuePair<string, string>("__VIEWSTATEGENERATOR", viewStateGenerator));
                keyValues.Add(new KeyValuePair<string, string>("txtProposal","dwellings"));
                keyValues.Add(new KeyValuePair<string, string>("rbGroup", "rbRange"));
                keyValues.Add(new KeyValuePair<string, string>("cboSelectDateValue", "DATE_RECEIVED"));
                keyValues.Add(new KeyValuePair<string, string>("cboDays", "1"));
                keyValues.Add(new KeyValuePair<string, string>("cboMonths", "1"));
                keyValues.Add(new KeyValuePair<string, string>("dateStart", "01-05-2017"));
                keyValues.Add(new KeyValuePair<string, string>("dateEnd", "28-02-2018"));
                keyValues.Add(new KeyValuePair<string, string>("edrDateSelection", ""));
                keyValues.Add(new KeyValuePair<string, string>("csbtnSearch", "Search"));

                var request = new HttpRequestMessage(HttpMethod.Post, "/Northgate/PlanningExplorer/KeywordsSearch.aspx");
                request.Content = new FormUrlEncodedContent(keyValues);
                //request.Content.Headers.Add("Content-Length", request.Content.ToString().Length.ToString());
                //request.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");

                client.DefaultRequestHeaders.Add("Origin", "http://planning.wiltshire.gov.uk");
                client.DefaultRequestHeaders.Add("Referer", "http://planning.wiltshire.gov.uk/Northgate/PlanningExplorer/KeywordsSearch.aspx");

                var searchPostResponse = client.SendAsync(request).GetAwaiter().GetResult();
                var searchPostResponseContent = searchPostResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                var searchPostResponseDoc = CsQuery.CQ.Create(searchPostResponseContent);
                var searchPostResponseAction = searchPostResponseDoc.Select("form").Attr("action");

                var startIndex = searchPostResponseAction.IndexOf("XMLLoc=", StringComparison.CurrentCulture);
                var endIndex = searchPostResponseAction.LastIndexOf(".xml", StringComparison.CurrentCulture) + 4;
                var XMLLoc = Uri.UnescapeDataString(searchPostResponseAction.Substring(startIndex, endIndex - startIndex));

                var nextPageAction = $"StdResults.aspx?PT=Planning Applications On-Line&PS=100&{XMLLoc}&FT=Planning Application Search Results&XSLTemplate=/Northgate/PlanningExplorer/SiteFiles/Skins/Wiltshire/xslt/PL/PLResults.xslt&p=10";

                var nextPageUrl = $"/Northgate/PlanningExplorer/Generic/{nextPageAction}";
                var searchNextPageResponse = client.GetAsync(nextPageUrl, CancellationToken.None).GetAwaiter().GetResult();
                var searchNextPageResponseContent = searchNextPageResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();

                //var cookieUri = new Uri("http://planning.wiltshire.gov.uk/Northgate/PlanningExplorer/KeywordsSearch.aspx");
                //IEnumerable<Cookie> responseCookies = cookieContainer.GetCookies(cookieUri).Cast<Cookie>();

                //var uri = "/Northgate/PlanningExplorer/Generic/StdResults.aspx?PT=Planning%20Applications%20On-Line&SC=Date%20Received%20is%20between%2001%20December%202017%20and%2028%20February%202018%20and%20Development%20Description%20contains%20DWELLINGS&FT=Planning%20Application%20Search%20Results&XMLSIDE=/Northgate/PlanningExplorer/SiteFiles/Skins/Wiltshire/Menus/PL.xml&XSLTemplate=/Northgate/PlanningExplorer/SiteFiles/Skins/Wiltshire/xslt/PL/PLResults.xslt&PS=10&XMLLoc=/Northgate/PlanningExplorer/generic/XMLtemp/hvbnr5iswpmj13bm3u3yzuu5/1cadb6c6-813f-4ff2-94df-f757aae5e63c.xml";

                client.DefaultRequestHeaders.Add("Referer", "http://planning.wiltshire.gov.uk/Northgate/PlanningExplorer/KeywordsSearch.aspx");
                client.DefaultRequestHeaders.Add("Cookie", "_ga=GA1.3.1095016530.1517476707; _gid=GA1.3.329444085.1517476707; ASCCPref=1; ASP.NET_SessionId=hvbnr5iswpmj13bm3u3yzuu5; MVMSession=ID=563f2b5b-c19d-46d0-9ea9-99c6d99e9eec");
                //cookieContainer.Add(baseAddress, new Cookie("Cookie", "_ga=GA1.3.1095016530.1517476707; _gid=GA1.3.329444085.1517476707; ASCCPref=1; _gat=1; _gat_UA-3240460-9UA-3240460-9=1; _gat_UA-3240460-9=1; ASP.NET_SessionId=hvbnr5iswpmj13bm3u3yzuu5; MVMSession=ID=563f2b5b-c19d-46d0-9ea9-99c6d99e9eec"));

                //var searchResponse = client.GetAsync(uri, CancellationToken.None).GetAwaiter().GetResult();
                //var responseContent = searchResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                //Console.WriteLine(responseContent);
            }
        }

        private static void SetDefaultHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.Add("Host", "planning.wiltshire.gov.uk");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Cache-Control", "max-age=0");
            client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/63.0.3239.132 Safari/537.36");
            client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9,en-US;q=0.8,fr;q=0.7");
        }
    }
}
