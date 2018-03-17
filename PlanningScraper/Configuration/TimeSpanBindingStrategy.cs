using System;
using System.Reflection;
using System.Xml;
using SimpleConfig;
using SimpleConfig.BindingStrategies;

namespace PlanningScraper.Configuration
{
    public class TimeSpanBindingStrategy : IBindingStrategy
    {
        private string _elementName;

        public TimeSpanBindingStrategy() { }
        public TimeSpanBindingStrategy(string elementName)
        {
            _elementName = elementName;
        }

        public bool Map(object destinationObject, PropertyInfo destinationProperty, XmlElement element, XmlElement allConfig, ConfigMapper mapper)
        {
            ((SystemConfig) destinationObject).RequestDelayTimeSpan = TimeSpan.Parse(element.ChildNodes[3].InnerText);
            return true;
        }
    }
}
