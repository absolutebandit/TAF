using PlanningScraper.Interfaces;

namespace PlanningScraper.Types
{
    public class SearcherType
    {
        public SearcherType(string searchArea, ISiteSearcher searcher)
        {
            this.SearchArea = searchArea;
            this.Searcher = searcher;
        }

        public string SearchArea { get; set; }

        public ISiteSearcher Searcher { get; set; }
    }
}
