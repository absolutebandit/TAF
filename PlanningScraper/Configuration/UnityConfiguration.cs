using System;
using System.Configuration;
using PlanningScraper.Interfaces;
using PlanningScraper.Output;
using PlanningScraper.Poole;
using PlanningScraper.Wiltshire;
using Unity;

namespace PlanningScraper.Configuration
{
    public static class UnityConfiguration
    {
        public static void RegisterComponents(IUnityContainer container, string[] site)
        {
            ISystemConfig systemConfig = (SystemConfig)(dynamic)ConfigurationManager.GetSection("systemConfig");
            container.RegisterInstance<ISystemConfig>(systemConfig);

            ISearchConfig searchConfig = (SearchConfig)(dynamic)ConfigurationManager.GetSection("searchConfig");
            container.RegisterInstance<ISearchConfig>(searchConfig);

            IWiltshireConfig wiltshireConfig = (WiltshireConfig)(dynamic)ConfigurationManager.GetSection("wiltshireConfig");
            container.RegisterInstance<IWiltshireConfig>(wiltshireConfig);

            IPooleConfig pooleConfig = (PooleConfig)(dynamic)ConfigurationManager.GetSection("pooleConfig");
            container.RegisterInstance<IPooleConfig>(pooleConfig);

            container.RegisterInstance<ILogger>(new Logger(container.Resolve<ISystemConfig>()));
            container.RegisterType<ISiteSearcher, WiltshireSearcher>("wiltshire");
            container.RegisterType<IPlanningDataExtractor, WiltshireExtractor>("wiltshire");
            container.RegisterType<ISiteSearcher, PooleSearcher>("poole");
            container.RegisterType<IPlanningDataExtractor, PooleExtractor>("poole");
            container.RegisterInstance<IFileWriter>(new FileWriter(container.Resolve<ILogger>(), container.Resolve<ISystemConfig>()));
        }
    }
}
