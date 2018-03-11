using System;

namespace PlanningScraper.Interfaces
{
    public interface IConfiguration
    {
        string SearchTerm { get; set; }
        string DateType { get; set; }
        string StartDate { get; set; }
        string EndDate { get; set; }
        string BaseUri { get; set; }
        string KeywordSearchRoute { get; set; }
        string ApplicationPageRoute { get; set; }
        string DefaultPageSize { get; set; }
        string DesiredPageSize { get; set; }
        string OutputFileName { get; set; }
        string LogFileName { get; set; }
        TimeSpan RequestDelayTimeSpan { get; set; }
        bool UseProxy { get; set; }
        string ProxyUserName { get; set; }
        string ProxyPassword { get; set; }
        string ProxyUri { get; set; }
    }
}
