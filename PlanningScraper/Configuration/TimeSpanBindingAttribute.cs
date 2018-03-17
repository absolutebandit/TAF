using System;
using SimpleConfig.BindingStrategies;

namespace PlanningScraper.Configuration
{
    [AttributeUsage(AttributeTargets.Property)]
    public class TimeSpanAttribute : BaseBindingAttribute
    {
        public TimeSpanAttribute() { }
        public TimeSpanAttribute(string elementName)
        {
            ElementName = elementName;
        }

        public string ElementName { get; private set; }

        public override IBindingStrategy MappingStrategy => new TimeSpanBindingStrategy(ElementName);
    }
}
