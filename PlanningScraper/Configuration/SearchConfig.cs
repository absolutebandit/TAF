using PlanningScraper.Interfaces;

namespace PlanningScraper.Configuration
{
    public class SearchConfig : ISearchConfig
    {
        public string SearchTerm { get; set; }
        public string DateType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }
}
