using System.Collections.Generic;

namespace RK.CalendarSync.Core.Configuration.Synchronization
{
    public interface ISynchronizationConfigurationReaderWriter
    {
        /// <summary>
        /// Get a list of all synchronization configurations.
        /// </summary>
        /// <returns></returns>
        IList<ISynchronizationConfiguration> GetSynchronizationConfigurations();

        /// <summary>
        /// Saves synchronization configurations.
        /// </summary>
        /// <param name="synchronizationConfigurations"></param>
        void SaveSynchronizationConfigurations(IList<ISynchronizationConfiguration> synchronizationConfigurations);
    }
}
