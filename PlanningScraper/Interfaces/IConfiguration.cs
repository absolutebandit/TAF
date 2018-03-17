using System;

namespace PlanningScraper.Interfaces
{
    public interface IConfiguration
    {
        string SearchTerm { get; set; }
        string BaseUri { get; set; }
        string KeywordSearchRoute { get; set; }
        string ApplicationPageRoute { get; set; }
        string DefaultPageSize { get; set; }
        string DesiredPageSize { get; set; }
        bool UseProxy { get; set; }
    }
}
