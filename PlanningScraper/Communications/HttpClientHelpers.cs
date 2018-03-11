using System.Net;
using System.Net.Http;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Communications
{
    public class HttpClientHelpers
    {
        public static HttpClient CreateClient(IConfiguration configuration, ILogger logger, CookieContainer cookieContainer)
        {
            var handler = CreateHttpClientHandler(configuration, cookieContainer);
            return new HttpClientWrapper(handler, logger);
        }

        public static HttpClientHandler CreateHttpClientHandler(IConfiguration configuration, CookieContainer cookieContainer)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false,
                UseProxy = configuration.UseProxy
            };

            if (handler.UseProxy)
            {
                handler.Proxy = new WebProxy(configuration);
            }

            return handler;
        }
    }
}
