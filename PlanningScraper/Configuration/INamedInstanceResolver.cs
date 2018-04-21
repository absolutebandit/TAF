namespace PlanningScraper.Configuration
{
    public interface INamedInstanceResolver<T>
    {
        T ResolveConfig(string name);
    }
}
