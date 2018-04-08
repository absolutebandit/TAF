namespace PlanningScraper.Interfaces
{
    public interface IBournemouthConfig : IConfiguration
    {
        string SearchResultsRoute { get; set; }

        int ChunkSizeDays { get; set; }
    }
}
