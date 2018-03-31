using System.Threading;
using System.Threading.Tasks;

namespace PlanningScraper.Interfaces
{
    public interface ISiteSearcher
    {
        Task<SearchDataAndResults> ExecuteSearchAsync(string searchArea, CancellationToken cancellationToken);
    }
}
