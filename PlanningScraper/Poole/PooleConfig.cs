using PlanningScraper.Interfaces;

namespace PlanningScraper.Poole
{
    public class PooleConfig : Configuration.Configuration, IPooleConfig
    {
        public string AdvancedSearchRoute { get; set; }
        public string PagedSearchResultsRoute { get; set; }
        public int ChunkSizeDays { get; set; }
    }
}
