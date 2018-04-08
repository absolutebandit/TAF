using System.Net;
using System.Net.Http;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Communications
{
    public class HttpClientHelpers
    {
        public static HttpClientWrapper CreateClient(string baseAddress, ISystemConfig systemConfig, IConfiguration configuration, ILogger logger, CookieContainer cookieContainer)
        {
            var handler = CreateHttpClientHandler(systemConfig, configuration, cookieContainer);
                     
            return new HttpClientWrapper(baseAddress, handler, logger, systemConfig);
        }

        public static HttpClientHandler CreateHttpClientHandler(ISystemConfig systemConfig, IConfiguration configuration, CookieContainer cookieContainer)
        {
            var handler = new HttpClientHandler()
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                CookieContainer = cookieContainer,
                AllowAutoRedirect = false,
                UseProxy = true
            };

            if (configuration.UseProxy)
            {
                handler.Proxy = new WebProxy(systemConfig);
            }

            return handler;
        }
    }
}
