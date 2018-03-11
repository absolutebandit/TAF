using System;

namespace PlanningScraper.Exceptions
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
