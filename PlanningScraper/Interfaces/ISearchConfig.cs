using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningScraper.Interfaces
{
    public interface ISearchConfig
    {
        string SearchTerm { get; set; }
        string DateType { get; set; }
        string StartDate { get; set; }
        string EndDate { get; set; }
    }
}
