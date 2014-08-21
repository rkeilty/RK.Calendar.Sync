using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RK.CalendarSync.Core.Configuration.Calendars
{
    /// <summary>
    /// Retrieves all calendar configurations from on disk XML file representations.
    /// </summary>
    internal class FileBasedCalendarConfigurationRetriever : ICalendarConfigurationRetriever
    {
        private const string DEFAULT_CONFIGURATION_FILE = "CalendarConfigurations.xml";
        private readonly string _configurationFile ;

        public FileBasedCalendarConfigurationRetriever() : this(DEFAULT_CONFIGURATION_FILE)
        {}

        public FileBasedCalendarConfigurationRetriever(string configurationFile)
        {
            _configurationFile = configurationFile;
        }


        /// <summary>
        /// Retrieves all of the calendar configurations
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, ICalendarConfiguration> GetCalendarConfigurations()
        {
            var xmlDocument = XDocument.Load(_configurationFile);
            var calendarElements = xmlDocument.Root.Elements();
            var configurationList = new Dictionary<Guid, ICalendarConfiguration>();

            foreach (var calendarElement in calendarElements)
            {
                var calendarType = calendarElement.Name.LocalName;

                var configuration = DeserializeCalendarConfiguration(calendarType, calendarElement);
                configurationList.Add(configuration.CalendarConfigurationId, configuration);
            }

            return configurationList;
        }

        /// <summary>
        /// Deserialize a specific calendar configuration node.
        /// </summary>
        /// <param name="calendarType"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private ICalendarConfiguration DeserializeCalendarConfiguration(string calendarType, XElement configuration)
        {
            Type serializationType = typeof(object);
            if (calendarType.Equals("GoogleCalendarConfiguration"))
            {
                serializationType = typeof (GoogleCalendarConfiguration);
            }
            else if (calendarType.Equals("ExchangeCalendarConfiguration"))
            {
                serializationType = typeof (ExchangeCalendarConfiguration);
            }
            else
            {
                throw new InvalidDataException("Unable to parse calendar configuration");
            }
            
            var serializer = new XmlSerializer(serializationType);
            return (ICalendarConfiguration)serializer.Deserialize(configuration.CreateReader());
        }
    }
}