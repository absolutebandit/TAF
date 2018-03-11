using System;
using System.Configuration;
using PlanningScraper.Interfaces;
using PlanningScraper.Output;
using PlanningScraper.Wiltshire;
using Unity;

namespace PlanningScraper.Configuration
{
    public static class UnityConfiguration
    {
        public static void RegisterComponents(IUnityContainer container)
        {
            container.RegisterInstance<IConfiguration>(new Configuration
            {
                SearchTerm = ConfigurationManager.AppSettings["searchTerm"],
                BaseUri = ConfigurationManager.AppSettings["baseUri"],
                ApplicationPageRoute = ConfigurationManager.AppSettings["applicationPageRoute"],
                KeywordSearchRoute = ConfigurationManager.AppSettings["keywordSearchRoute"],
                DateType = ConfigurationManager.AppSettings["dateType"],
                StartDate = ConfigurationManager.AppSettings["startDate"],
                EndDate = ConfigurationManager.AppSettings["endDate"],
                DefaultPageSize = ConfigurationManager.AppSettings["defaultPageSize"],
                DesiredPageSize = ConfigurationManager.AppSettings["desiredPageSize"],
                LogFileName = ConfigurationManager.AppSettings["logFileName"],
                OutputFileName = ConfigurationManager.AppSettings["outputFileName"],
                RequestDelayTimeSpan = TimeSpan.Parse(ConfigurationManager.AppSettings["requestDelayTimeSpan"]),
                UseProxy = Boolean.Parse(ConfigurationManager.AppSettings["useProxy"]),
                ProxyUserName = ConfigurationManager.AppSettings["proxyUserName"],
                ProxyPassword = ConfigurationManager.AppSettings["proxyPassword"],
                ProxyUri = ConfigurationManager.AppSettings["proxyUri"]
            });

            container.RegisterInstance<ILogger>(new Logger(container.Resolve<IConfiguration>()));
            container.RegisterType<ISiteSearcher, WiltshireSearcher>("wiltshire");
            container.RegisterType<IPlanningDataExtractor, WiltshireExtractor>("wiltshire");
            container.RegisterInstance<IFileWriter>(new FileWriter(container.Resolve<ILogger>(), container.Resolve<IConfiguration>()));
        }
    }
}
