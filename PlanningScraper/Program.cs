using System;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Configuration;
using PlanningScraper.Exceptions;
using PlanningScraper.Interfaces;
using Unity;

namespace PlanningScraper
{
    class Program
    {
        private readonly IUnityContainer _container = new UnityContainer();
        private ILogger _logger;
        private ISiteSearcher _searcher;
        private IPlanningDataExtractor _extractor;
        private IFileWriter _fileWriter;
        private CancellationToken _cancellationToken;

        static void Main(string[] args)
        {
            var site = args[0];
            var program = new Program();
            program.Run(site).GetAwaiter().GetResult();
        }

        private async Task Run(string site)
        {
            try
            {
                Initialise(site);

                var searchDataAndResults = await _searcher.ExecuteSearchAsync(_cancellationToken);
                var planningApplications = await _extractor.ExtractDataAsync(searchDataAndResults.SearchResultsPages, searchDataAndResults.CookieContainer, _cancellationToken);
                await _fileWriter.WriteOutputFileAsync(planningApplications, _cancellationToken);
            }
            catch (SearchFailedException sfe)
            {
                await _logger.LogExceptionAsync(ExceptionMessages.SearchFailedMessage, sfe, _cancellationToken);
            }
            catch (ExtractDataFailedException ede)
            {
                await _logger.LogExceptionAsync(ExceptionMessages.DataExtractFailedMessage, ede, _cancellationToken);
            }
            catch (Exception ex)
            {
                await _logger.LogExceptionAsync(ExceptionMessages.GeneralFailureMessage, ex, _cancellationToken);
            }
        }

        private void Initialise(string site)
        {
            _cancellationToken = new CancellationToken();
            UnityConfiguration.RegisterComponents(_container);
            _logger = _container.Resolve<ILogger>();
            _searcher = _container.Resolve<ISiteSearcher>(site.ToLower());
            _extractor = _container.Resolve<IPlanningDataExtractor>(site.ToLower());
            _fileWriter = _container.Resolve<IFileWriter>();
        }
    }
}
