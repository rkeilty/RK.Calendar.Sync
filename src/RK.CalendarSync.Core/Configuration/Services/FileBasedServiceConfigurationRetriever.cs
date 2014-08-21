using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RK.CalendarSync.Core.Configuration.Services
{
    /// <summary>
    /// Retrieves all service configurations from on disk XML file representations.
    /// </summary>
    internal class FileBasedServiceConfigurationRetriever : IServiceConfigurationRetriever
    {
        private const string DEFAULT_CONFIGURATION_FILE = "ServiceConfigurations.xml";
        private readonly string _configurationFile ;

        public FileBasedServiceConfigurationRetriever() : this(DEFAULT_CONFIGURATION_FILE)
        {}

        public FileBasedServiceConfigurationRetriever(string configurationFile)
        {
            _configurationFile = configurationFile;
        }


        /// <summary>
        /// Retrieves all of the service configurations
        /// </summary>
        /// <returns></returns>
        public IDictionary<CalendarType, IServiceConfiguration> GetServiceConfigurations()
        {
            var xmlDocument = XDocument.Load(_configurationFile);
            var serviceElements = xmlDocument.Root.Elements();
            var configurationList = new Dictionary<CalendarType, IServiceConfiguration>();

            foreach (var serviceElement in serviceElements)
            {
                var calendarType = serviceElement.Name.LocalName;

                var configuration = DeserializeServiceConfiguration(calendarType, serviceElement);
                configurationList.Add(configuration.CalendarType, configuration);
            }

            return configurationList;
        }

        /// <summary>
        /// Deserialize a specific service configuration node.
        /// </summary>
        /// <param name="calendarType"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        private IServiceConfiguration DeserializeServiceConfiguration(string calendarType, XElement configuration)
        {
            Type serializationType = typeof(object);
            if (calendarType.Equals("GoogleServiceConfiguration"))
            {
                serializationType = typeof(GoogleServiceConfiguration);
            }
            else
            {
                throw new InvalidDataException("Unknown type for service configuration.");
            }

            var serializer = new XmlSerializer(serializationType);
            return (IServiceConfiguration)serializer.Deserialize(configuration.CreateReader());
        }
    }
}