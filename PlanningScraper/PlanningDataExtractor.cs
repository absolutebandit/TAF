using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PlanningScraper
{
    public class PlanningDataExtractor
    {
        List<PlanningApplication> _planningApplications = new List<PlanningApplication>();

        public IEnumerable<PlanningApplication> ExtractData(HttpResponseMessage searchResults)
        {
            var searchResultsString = searchResults.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            System.IO.File.WriteAllText(@"c:\samplesearchresult.txt", searchResultsString);

            return _planningApplications;
        }
    }
}
