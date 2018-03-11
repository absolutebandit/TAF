using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningScraper.Interfaces
{
    public interface IFileWriter
    {
        Task WriteOutputFileAsync(IEnumerable<PlanningApplication> planningApplications, CancellationToken cancellationToken);
    }
}
