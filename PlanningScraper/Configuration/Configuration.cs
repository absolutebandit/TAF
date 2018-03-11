using System;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Configuration
{
    public class Configuration : IConfiguration
    {
        public string SearchTerm { get; set; }
        public string DateType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string BaseUri { get; set; }
        public string KeywordSearchRoute { get; set; }
        public string ApplicationPageRoute { get; set; }
        public string DefaultPageSize { get; set; }
        public string DesiredPageSize { get; set; }
        public string OutputFileName { get; set; }
        public string LogFileName { get; set; }
        public TimeSpan RequestDelayTimeSpan { get; set; }
        public bool UseProxy { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }
        public string ProxyUri { get; set; }
    }
}
