using System;
using System.Threading;
using System.Threading.Tasks;

namespace PlanningScraper.Interfaces
{
    public interface ILogger
    {
        Task LogInformationAsync(string message, CancellationToken cancellationToken);

        Task LogExceptionAsync(string message, Exception exception, CancellationToken cancellationToken);
    }
}
