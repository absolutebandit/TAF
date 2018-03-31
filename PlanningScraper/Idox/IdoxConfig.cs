using PlanningScraper.Interfaces;

namespace PlanningScraper.Idox
{
    public class IdoxConfig : Configuration.Configuration, IIdoxConfig
    {
        public string AdvancedSearchRoute { get; set; }
        public string PagedSearchResultsRoute { get; set; }
        public int ChunkSizeDays { get; set; }
    }
}
