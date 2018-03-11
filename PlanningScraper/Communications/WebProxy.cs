using System;
using System.Net;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Communications
{
    public class WebProxy : IWebProxy
    {
        private readonly IConfiguration _configuration;

        public WebProxy(IConfiguration configuration)
        {
            _configuration = configuration;
            this.Credentials = new NetworkCredential(string.Format(configuration.ProxyUserName, new Random().Next(10000).ToString().PadLeft(5, '0')), configuration.ProxyPassword);
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination) { return new Uri(_configuration.ProxyUri); }

        public bool IsBypassed(Uri host) { return false; }
    }
}
