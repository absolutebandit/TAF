using System.Configuration;
using PlanningScraper.Bournemouth;
using PlanningScraper.Idox;
using PlanningScraper.Interfaces;
using PlanningScraper.Output;
using PlanningScraper.Wiltshire;
using Unity;
using IdoxConfig = PlanningScraper.Idox.IdoxConfig;

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

            IdoxConfig idoxConfig = (IdoxConfig)(dynamic)ConfigurationManager.GetSection("idoxConfig");
            container.RegisterInstance<IIdoxConfig>(idoxConfig);

            BournemouthConfig bournemouthConfig = (BournemouthConfig)(dynamic)ConfigurationManager.GetSection("bournemouthConfig");
            container.RegisterInstance<IBournemouthConfig>(bournemouthConfig);

            container.RegisterInstance<ILogger>(new Logger(container.Resolve<ISystemConfig>()));
            container.RegisterType<ISiteSearcher, WiltshireSearcher>("wiltshire");
            container.RegisterType<IPlanningDataExtractor, WiltshireExtractor>("wiltshire");
            container.RegisterType<ISiteSearcher, IdoxSearcher>("poole");
            container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("poole");
            container.RegisterType<ISiteSearcher, BournemouthSearcher>("bournemouth");
            container.RegisterType<IPlanningDataExtractor, BournemouthExtractor>("bournemouth");
            container.RegisterInstance<IFileWriter>(new FileWriter(container.Resolve<ILogger>(), container.Resolve<ISystemConfig>()));
        }
    }
}
