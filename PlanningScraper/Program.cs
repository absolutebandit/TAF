using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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
        private readonly List<ISiteSearcher> _searchers = new List<ISiteSearcher>();
        private readonly List<IPlanningDataExtractor> _extractors = new List<IPlanningDataExtractor>();
        private readonly List<PlanningApplication> _planningApplications = new List<PlanningApplication>();
        private readonly IUnityContainer _container = new UnityContainer();
        private ILogger _logger;
        private IFileWriter _fileWriter;
        private CancellationToken _cancellationToken;

        static void Main(string[] args)
        {
            var areas = args;
            var program = new Program();
            program.Run(areas).GetAwaiter().GetResult();
        }

        private async Task Run(string[] areas)
        {
            try
            {
                if (await Initialise(areas, _cancellationToken))
                {
                    for (var i = 0; i < _searchers.Count; i++)
                    {
                        var searchDataAndResults = await _searchers[i].ExecuteSearchAsync(_cancellationToken);
                        var planningApplications = await _extractors[i].ExtractDataAsync(searchDataAndResults.SearchResultsPages, searchDataAndResults.CookieContainer, _cancellationToken);
                        _planningApplications.AddRange(planningApplications);
                    }
                    
                    await _fileWriter.WriteOutputFileAsync(_planningApplications, _cancellationToken);
                }
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

        private async Task<bool> Initialise(string[] areas, CancellationToken cancellationToken)
        {
            _cancellationToken = new CancellationToken();
            UnityConfiguration.RegisterComponents(_container, areas);
            _logger = _container.Resolve<ILogger>();
            _fileWriter = _container.Resolve<IFileWriter>();

            if (areas == null || areas.Length == 0)
            {
                await _logger.LogInformationAsync("You must enter at least one area!", cancellationToken);
                return false;
            }

            foreach (var area in areas)
            {
                var allSites = _container.Resolve<ISystemConfig>().SupportedAreas.ToLower().Split(',').ToList();

                if (area.Trim().ToLower() != "all" && !allSites.Contains(area.Trim().ToLower()))
                {
                    await _logger.LogInformationAsync($"Invalid area entered {area}", cancellationToken);
                    return false;
                }

                if (area.Trim().ToLower() == "all")
                {

                    foreach (var ar in allSites)
                    {
                        _searchers.Add(_container.Resolve<ISiteSearcher>(ar.Trim().ToLower()));
                        _extractors.Add(_container.Resolve<IPlanningDataExtractor>(ar.Trim().ToLower()));
                    }
                }


                _searchers.Add(_container.Resolve<ISiteSearcher>(area.Trim().ToLower()));
                _extractors.Add(_container.Resolve<IPlanningDataExtractor>(area.Trim().ToLower()));
            }

            return true;
        }
    }
}
