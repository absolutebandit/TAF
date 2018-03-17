using PlanningScraper.Interfaces;

namespace PlanningScraper.Configuration
{
    public class SearchConfig : ISearchConfig
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
