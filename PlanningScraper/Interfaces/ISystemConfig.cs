using System;

namespace PlanningScraper.Interfaces
{
    public interface ISystemConfig
    {
        string SupportedAreas { get; set; }
        string OutputFileName { get; set; }
        string LogFileName { get; set; }
        TimeSpan RequestDelayTimeSpan { get; set; }
        string ProxyUserName { get; set; }
        string ProxyPassword { get; set; }
        string ProxyUri { get; set; }
    }
}
