using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningScraper.Interfaces
{
    public interface ISiteSearcher
    {
        Task<SearchDataAndResults> ExecuteSearchAsync(CancellationToken cancellationToken);
    }
}
