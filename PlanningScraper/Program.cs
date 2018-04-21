using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Configuration;
using PlanningScraper.Exceptions;
using PlanningScraper.Interfaces;
using PlanningScraper.Types;
using Unity;

namespace PlanningScraper
{
    class Program
    {
        private readonly List<SearcherType> _searchers = new List<SearcherType>();
        private readonly List<ExtractorType> _extractors = new List<ExtractorType>();
        private readonly List<PlanningApplication> _planningApplications = new List<PlanningApplication>();
        private readonly IUnityContainer _container = new UnityContainer();
        private ILogger _logger;
        private IFileWriter _fileWriter;
        private CancellationToken _cancellationToken;


        static void Main(string[] args)
        {
            var areaList = FormatAreaList(args);
            var program = new Program();
            program.Run(areaList).GetAwaiter().GetResult();
        }

        private static List<string> FormatAreaList(string[] args)
        {
            var systemConfig = (SystemConfig)(dynamic)ConfigurationManager.GetSection("systemConfig");

            var areaList = new List<string>();
            foreach (var area in args)
            {
                areaList.Add(area.Trim().ToLower());
            }

            if (areaList.Contains("all"))
            {
                var allAreas = systemConfig.SupportedAreas.ToLower().Split(',').ToList();
                areaList = new List<string>();

                foreach (var area in allAreas)
                {
                    areaList.Add(area.Trim());    
                }
            }

            return areaList;
        }

        private async Task Run(List<string> areaList)
        {
            try
            {
                if (await Initialise(areaList, _cancellationToken))
                {
                    for (var i = 0; i < _searchers.Count; i++)
                    {
                        var searchDataAndResults = await _searchers[i].Searcher.ExecuteSearchAsync(_searchers[i].SearchArea,  _cancellationToken);
                        var planningApplications = await _extractors[i].Extractor.ExtractDataAsync(_extractors[i].SearchArea, searchDataAndResults.SearchResultsPages, searchDataAndResults.CookieContainer, _cancellationToken);
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

        private async Task<bool> Initialise(List<string> areaList, CancellationToken cancellationToken)
        {
            _cancellationToken = new CancellationToken();
            UnityConfiguration.RegisterComponents(_container, areaList);
            _logger = _container.Resolve<ILogger>();
            _fileWriter = _container.Resolve<IFileWriter>();

            if (!await ValidateInputs(areaList, cancellationToken)) return false;

            if (!await CreateSearchersAndExtractors(areaList, cancellationToken)) return false;

            return true;
        }
        
        private async Task<bool> ValidateInputs(List<string> areaList, CancellationToken cancellationToken)
        {
            var searchConfig = _container.Resolve<ISearchConfig>();
            var startDate = DateTime.ParseExact(searchConfig.StartDate, "dd-MM-yyyy", null);
            var endDate = DateTime.ParseExact(searchConfig.EndDate, "dd-MM-yyyy", null);
            var today = DateTime.ParseExact(DateTime.Today.ToString("dd-MM-yyyy"), "dd-MM-yyyy", null);
            if (startDate > today || endDate > today)
            {
                await _logger.LogInformationAsync(
                    "Date configuration invalid, startDate must be equal to today or earlier, end date must be equal to today or earlier.",
                    cancellationToken);
                return false;
            }

            if (endDate < startDate)
            {
                await _logger.LogInformationAsync("Date configuration invalid, end date is earlier than start date.",
                    cancellationToken);
                return false;
            }

            if (areaList == null || areaList.Count == 0)
            {
                await _logger.LogInformationAsync("You must enter at least one area!", cancellationToken);
                return false;
            }

            return true;
        }

        private async Task<bool> CreateSearchersAndExtractors(IEnumerable<string> areaList, CancellationToken cancellationToken)
        {
            foreach (var area in areaList)
            {
                var allAreas = _container.Resolve<ISystemConfig>().SupportedAreas.ToLower().Split(',').ToList();

                if (!allAreas.Contains(area))
                {
                    await _logger.LogInformationAsync($"Invalid area entered {area}", cancellationToken);
                    return false;
                }

                CreateSearcher(area);
                CreateExtractor(area);
            }

            return true;
        }

        private void CreateExtractor(string area)
        {
            var extractor = new ExtractorType(area, _container.Resolve<IPlanningDataExtractor>(area));
            _extractors.Add(extractor);
        }

        private void CreateSearcher(string area)
        {
            var searcher = new SearcherType(area, _container.Resolve<ISiteSearcher>(area));
            _searchers.Add(searcher);
        }
    }
}
