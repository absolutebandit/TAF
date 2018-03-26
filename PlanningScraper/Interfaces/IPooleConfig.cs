namespace PlanningScraper.Interfaces
{
    public interface IPooleConfig : IConfiguration
    {
        string AdvancedSearchRoute { get; set; }
        string PagedSearchResultsRoute { get; set; }
        int ChunkSizeDays { get; set; }
    }
}
