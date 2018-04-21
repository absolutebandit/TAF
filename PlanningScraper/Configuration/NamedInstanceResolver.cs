using Unity;

namespace PlanningScraper.Configuration
{
    public class NamedInstanceResolver<T> : INamedInstanceResolver<T>
    {
        private readonly IUnityContainer _container;

        public NamedInstanceResolver(IUnityContainer container)
        {
            _container = container;
        }

        public T ResolveConfig(string name)
        {
            var config = _container.Resolve<T>(name);
            return config;
        }
    }
}
