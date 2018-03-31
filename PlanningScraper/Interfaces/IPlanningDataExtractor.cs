using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningScraper.Interfaces
{
    public interface IPlanningDataExtractor
    {
        Task<IEnumerable<PlanningApplication>> ExtractDataAsync(string area, List<HttpResponseMessage> searchResultPages, CookieContainer cookieContainer, CancellationToken cancellationToken);
    }
}