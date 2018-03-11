using System;

namespace PlanningScraper.Exceptions
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
