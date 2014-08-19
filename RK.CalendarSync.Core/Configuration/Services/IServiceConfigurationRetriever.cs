using System.Collections.Generic;

namespace RK.CalendarSync.Core.Configuration.Services
{
    public interface IServiceConfigurationRetriever
    {
        /// <summary>
        /// Retrieves all service configurations
        /// </summary>
        /// <returns></returns>
        IDictionary<CalendarType, IServiceConfiguration> GetServiceConfigurations();
    }
}