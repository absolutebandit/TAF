using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlanningScraper
{
    public class ExtractDataFailedException : Exception
    {
        public ExtractDataFailedException()
        {
        }

        public ExtractDataFailedException(string message)
            : base(message)
        {
        }

        public ExtractDataFailedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
