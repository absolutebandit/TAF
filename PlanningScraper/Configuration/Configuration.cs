using PlanningScraper.Interfaces;

namespace PlanningScraper.Configuration
{
    public class Configuration : IConfiguration
    {
        public string SearchTerm { get; set; }
        public string BaseUri { get; set; }
        public string SearchRoute { get; set; }
        public string ApplicationPageRoute { get; set; }
        public string DefaultPageSize { get; set; }
        public string DesiredPageSize { get; set; }
        public bool UseProxy { get; set; }
    }
}
