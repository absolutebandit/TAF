using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Types
{
    public class ExtractorType
    {
        public ExtractorType(string searchArea, IPlanningDataExtractor extractor)
        {
            this.SearchArea = searchArea;
            this.Extractor = extractor;
        }

        public string SearchArea { get; set; }

        public IPlanningDataExtractor Extractor { get; set; }
    }
}
