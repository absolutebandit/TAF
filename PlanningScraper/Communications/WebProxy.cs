using System;
using System.Net;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Communications
{
    public class WebProxy : IWebProxy
    {
        private readonly ISystemConfig _systemConfig;

        public WebProxy(ISystemConfig systemConfig)
        {
            _systemConfig = systemConfig;
            this.Credentials = new NetworkCredential(string.Format(systemConfig.ProxyUserName, new Random().Next(10000).ToString().PadLeft(5, '0')), systemConfig.ProxyPassword);
        }

        public ICredentials Credentials { get; set; }

        public Uri GetProxy(Uri destination) { return new Uri(_systemConfig.ProxyUri); }

        public bool IsBypassed(Uri host) { return false; }
    }
}
