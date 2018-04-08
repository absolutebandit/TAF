using PlanningScraper.Interfaces;

namespace PlanningScraper.Bournemouth
{
    public class BournemouthConfig : Configuration.Configuration, IBournemouthConfig
    {
        public string SearchResultsRoute { get; set; }
        public int ChunkSizeDays { get; set; }
    }
}
