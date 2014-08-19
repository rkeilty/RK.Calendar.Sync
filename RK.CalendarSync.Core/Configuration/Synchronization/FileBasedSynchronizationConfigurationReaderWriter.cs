using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RK.CalendarSync.Core.Configuration.Synchronization
{
    /// <summary>
    /// Pulls configuration data from on disk XML files
    /// </summary>
    internal class FileBasedSynchronizationConfigurationReaderWriter : ISynchronizationConfigurationReaderWriter
    {
        private const string DEFAULT_CONFIGURATION_FILE = "SyncConfigurations.xml";
        private readonly string _configurationFile ;

        public FileBasedSynchronizationConfigurationReaderWriter() : this(DEFAULT_CONFIGURATION_FILE)
        {}

        internal FileBasedSynchronizationConfigurationReaderWriter(string configurationFile)
        {
            _configurationFile = configurationFile;
        }

        /// <summary>
        /// Get a list of all synchronization configurations.
        /// </summary>
        /// <returns></returns>
        public IList<ISynchronizationConfiguration> GetSynchronizationConfigurations()
        {
            var xmlDocument = XDocument.Load(_configurationFile);
            var syncElements = xmlDocument.Root.Elements();
            var configurationList = new List<ISynchronizationConfiguration>();

            var serializer = new XmlSerializer(typeof(SynchronizationConfiguration));

            foreach (var syncElement in syncElements)
            {
                var configuration = (ISynchronizationConfiguration)serializer.Deserialize(syncElement.CreateReader());
                configurationList.Add(configuration);
            }

            return configurationList;
        }


        /// <summary>
        /// Saves synchronization configurations.
        /// </summary>
        /// <param name="synchronizationConfigurations"></param>
        public void SaveSynchronizationConfigurations(IList<ISynchronizationConfiguration> synchronizationConfigurations)
        {
            var serializer = new XmlSerializer(typeof (SynchronizationConfiguration));
            using (var fileStream = new FileStream(_configurationFile, FileMode.Create))
            {
                using (var xmlWriter = new XmlTextWriter(fileStream, Encoding.UTF8))
                {
                    xmlWriter.WriteStartElement("SynchronizationConfigurations");

                    foreach (var syncConfig in synchronizationConfigurations)
                    {
                        serializer.Serialize(xmlWriter, syncConfig);
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.Close();
                }
            }
        }
    }
}
