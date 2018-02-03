using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningScraper
{
    public class SearchFailedException : Exception
    {
        public SearchFailedException()
        {
        }

        public SearchFailedException(string message)
            : base(message)
        {
        }

        public SearchFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
