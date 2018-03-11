using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace PlanningScraper
{
    public class SearchDataAndResults
    {
        public List<HttpResponseMessage> SearchResultsPages { get; set; }

        public CookieContainer CookieContainer { get; set; }
    }
}
