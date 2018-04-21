using System.Collections.Generic;
using System.Configuration;
using PlanningScraper.Bournemouth;
using PlanningScraper.Idox;
using PlanningScraper.Interfaces;
using PlanningScraper.Output;
using PlanningScraper.Wiltshire;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Registration;

namespace PlanningScraper.Configuration
{
    public static class UnityConfiguration
    {
        public static void RegisterComponents(IUnityContainer container, List<string> areaList)
        {
            RegisterGenericConfig(container);
            RegisterOutputs(container);
            RegisterAreas(container, areaList);
        }

        private static void RegisterOutputs(IUnityContainer container)
        {
            container.RegisterInstance<ILogger>(new Logger(container.Resolve<ISystemConfig>()));
            container.RegisterInstance<IFileWriter>(new FileWriter(container.Resolve<ILogger>(),
                container.Resolve<ISystemConfig>()));
        }

        private static void RegisterGenericConfig(IUnityContainer container)
        {
            ISystemConfig systemConfig = (SystemConfig) (dynamic) ConfigurationManager.GetSection("systemConfig");
            container.RegisterInstance<ISystemConfig>(systemConfig);
            ISearchConfig searchConfig = (SearchConfig) (dynamic) ConfigurationManager.GetSection("searchConfig");
            container.RegisterInstance<ISearchConfig>(searchConfig);
            container.RegisterInstance<INamedInstanceResolver<IIdoxConfig>>(new NamedInstanceResolver<IIdoxConfig>(container));
        }

        private static void RegisterAreas(IUnityContainer container, List<string> areaList)
        {
            if (areaList.Contains("wiltshire"))
            {
                var wiltshireConfig = (WiltshireConfig) (dynamic) ConfigurationManager.GetSection("wiltshireConfig");
                container.RegisterInstance<IWiltshireConfig>(wiltshireConfig);
                container.RegisterType<ISiteSearcher, WiltshireSearcher>("wiltshire");
                container.RegisterType<IPlanningDataExtractor, WiltshireExtractor>("wiltshire");
            }

            if (areaList.Contains("poole"))
            {
                var pooleIdoxConfig = (PooleIdoxConfig) (dynamic) ConfigurationManager.GetSection("pooleIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("poole", pooleIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("poole");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("poole");
            }

            if (areaList.Contains("gosport"))
            {
                var gosportIdoxConfig = (GosportIdoxConfig)(dynamic)ConfigurationManager.GetSection("gosportIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("gosport", gosportIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("gosport");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("gosport");
            }

            if (areaList.Contains("newforest"))
            {
                var newforestIdoxConfig = (NewForestIdoxConfig)(dynamic)ConfigurationManager.GetSection("newforestIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("newforest", newforestIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("newforest");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("newforest");
            }

            if (areaList.Contains("portsmouth"))
            {
                var portsmouthIdoxConfig = (PortsmouthIdoxConfig)(dynamic)ConfigurationManager.GetSection("portsmouthIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("portsmouth", portsmouthIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("portsmouth");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("portsmouth");
            }

            if (areaList.Contains("southampton"))
            {
                var southamptonIdoxConfig = (SouthamptonIdoxConfig)(dynamic)ConfigurationManager.GetSection("southamptonIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("southampton", southamptonIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("southampton");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("southampton");
            }

            if (areaList.Contains("swindon"))
            {
                var swindonIdoxConfig = (SwindonIdoxConfig)(dynamic)ConfigurationManager.GetSection("swindonIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("swindon", swindonIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("swindon");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("swindon");
            }

            if (areaList.Contains("winchester"))
            {
                var winchesterIdoxConfig = (WinchesterIdoxConfig)(dynamic)ConfigurationManager.GetSection("winchesterIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("winchester", winchesterIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("winchester");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("winchester");
            }

            if (areaList.Contains("testvalley"))
            {
                var testvalleyIdoxConfig = (TestValleyIdoxConfig)(dynamic)ConfigurationManager.GetSection("testvalleyIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("testvalley", testvalleyIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("testvalley");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("testvalley");
            }

            if (areaList.Contains("basingstoke"))
            {
                var basingstokeIdoxConfig = (BasingstokeIdoxConfig)(dynamic)ConfigurationManager.GetSection("basingstokeIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("basingstoke", basingstokeIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("basingstoke");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("basingstoke");
            }

            if (areaList.Contains("easthants"))
            {
                var easthantsIdoxConfig = (EastHantsIdoxConfig)(dynamic)ConfigurationManager.GetSection("easthantsIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("easthants", easthantsIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("easthants");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("easthants");
            }

            if (areaList.Contains("bristol"))
            {
                var bristolIdoxConfig = (BristolIdoxConfig)(dynamic)ConfigurationManager.GetSection("bristolIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("bristol", bristolIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("bristol");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("bristol");
            }

            if (areaList.Contains("cheltenham"))
            {
                var cheltenhamIdoxConfig = (CheltenhamIdoxConfig)(dynamic)ConfigurationManager.GetSection("cheltenhamIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("cheltenham", cheltenhamIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("cheltenham");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("cheltenham");
            }

            if (areaList.Contains("cornwall"))
            {
                var cornwallIdoxConfig = (CornwallIdoxConfig)(dynamic)ConfigurationManager.GetSection("cornwallIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("cornwall", cornwallIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("cornwall");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("cornwall");
            }

            if (areaList.Contains("cotswold"))
            {
                var cotswoldIdoxConfig = (CotswoldIdoxConfig)(dynamic)ConfigurationManager.GetSection("cotswoldIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("cotswold", cotswoldIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("cotswold");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("cotswold");
            }

            if (areaList.Contains("eastdevon"))
            {
                var eastDevonIdoxConfig = (EastDevonIdoxConfig)(dynamic)ConfigurationManager.GetSection("eastdevonIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("eastdevon", eastDevonIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("eastdevon");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("eastdevon");
            }

            if (areaList.Contains("gloucestershire"))
            {
                var gloucestershireIdoxConfig = (GloucestershireIdoxConfig)(dynamic)ConfigurationManager.GetSection("gloucestershireIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("gloucestershire", gloucestershireIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("gloucestershire");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("gloucestershire");
            }

            if (areaList.Contains("middevon"))
            {
                var middevonIdoxConfig = (GloucestershireIdoxConfig)(dynamic)ConfigurationManager.GetSection("middevonIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("middevon", middevonIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("middevon");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("middevon");
            }

            if (areaList.Contains("tewkesbury"))
            {
                var tewkesburyIdoxConfig = (TewkesburyIdoxConfig)(dynamic)ConfigurationManager.GetSection("tewkesburyIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("tewkesbury", tewkesburyIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("tewkesbury");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("tewkesbury");
            }

            if (areaList.Contains("torbay"))
            {
                var torbayIdoxConfig = (TorbayIdoxConfig)(dynamic)ConfigurationManager.GetSection("torbayIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("torbay", torbayIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("torbay");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("torbay");
            }

            if (areaList.Contains("westsomerset"))
            {
                var westSomersetIdoxConfig = (WestSomersetIdoxConfig)(dynamic)ConfigurationManager.GetSection("westsomersetIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("westsomerset", westSomersetIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("westsomerset");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("westsomerset");
            }

            if (areaList.Contains("buckinghamshire"))
            {
                var buckinghamshireIdoxConfig = (BuckinghamshireIdoxConfig)(dynamic)ConfigurationManager.GetSection("buckinghamshireIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("buckinghamshire", buckinghamshireIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("buckinghamshire");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("buckinghamshire");
            }

            if (areaList.Contains("mendip"))
            {
                var mendipIdoxConfig = (MendipIdoxConfig)(dynamic)ConfigurationManager.GetSection("mendipIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("mendip", mendipIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("mendip");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("mendip");
            }

            if (areaList.Contains("westberkshire"))
            {
                var westBerkshireIdoxConfig = (WestBerkshireIdoxConfig)(dynamic)ConfigurationManager.GetSection("westberkshireIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("westberkshire", westBerkshireIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("westberkshire");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("westberkshire");
            }

            if (areaList.Contains("southgloucestershire"))
            {
                var southGloucestershireIdoxConfig = (WestBerkshireIdoxConfig)(dynamic)ConfigurationManager.GetSection("southgloucestershireIdoxConfig");
                container.RegisterInstance<IIdoxConfig>("southgloucestershire", southGloucestershireIdoxConfig, new ContainerControlledLifetimeManager());
                container.RegisterType<ISiteSearcher, IdoxSearcher>("southgloucestershire");
                container.RegisterType<IPlanningDataExtractor, IdoxExtractor>("southgloucestershire");
            }

            if (areaList.Contains("bournemouth"))
            {
                var bournemouthConfig = (BournemouthConfig) (dynamic) ConfigurationManager.GetSection("bournemouthConfig");
                container.RegisterInstance<IBournemouthConfig>(bournemouthConfig);
                container.RegisterType<ISiteSearcher, BournemouthSearcher>("bournemouth");
                container.RegisterType<IPlanningDataExtractor, BournemouthExtractor>("bournemouth");
            }
        }
    }
}
