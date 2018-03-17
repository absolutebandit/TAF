using System;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Configuration
{
    public class SystemConfig : ISystemConfig
    {
        public string SupportedAreas { get; set; }
        public string OutputFileName { get; set; }
        public string LogFileName { get; set; }
        [TimeSpan]
        public TimeSpan RequestDelayTimeSpan { get; set; }
        public string ProxyUserName { get; set; }
        public string ProxyPassword { get; set; }
        public string ProxyUri { get; set; }
    }
}
