namespace PlanningScraper.Extensions
{
    public static class StringExtensions
    {
        public static string Clean(this string stringToClean)
        {
            return stringToClean?.Replace(",", ";")
                .Replace("\t", " ")
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Trim();
        }

        public static string CleanApplicationUrl(this string urlToClean)
        {
            return urlToClean.Replace(",", ";")
                .Replace("\t", string.Empty)
                .Replace("\r\n", string.Empty)
                .Replace("\n", string.Empty)
                .Trim();
        }
    }
}
