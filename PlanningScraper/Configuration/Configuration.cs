﻿using PlanningScraper.Interfaces;

namespace PlanningScraper.Configuration
{
    public class Configuration : IConfiguration
    {
        public string BaseUri { get; set; }
        public string KeywordSearchRoute { get; set; }
        public string ApplicationPageRoute { get; set; }
        public string DefaultPageSize { get; set; }
        public string DesiredPageSize { get; set; }
        public bool UseProxy { get; set; }
    }
}
