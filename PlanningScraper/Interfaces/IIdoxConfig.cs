namespace PlanningScraper.Interfaces
{
    public interface IIdoxConfig : IConfiguration
    {
        string AdvancedSearchRoute { get; set; }
        string PagedSearchResultsRoute { get; set; }
        int ChunkSizeDays { get; set; }
    }
}
