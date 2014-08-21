using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RK.CalendarSync.Core.Calendars.Factories;
using RK.CalendarSync.Core.Configuration.Calendars;
using RK.CalendarSync.Core.Configuration.Services;
using RK.CalendarSync.Core.Configuration.Synchronization;

namespace RK.CalendarSync.Core
{
    internal class CalendarSynchronizationWorkerFactory : ICalendarSynchronizationWorkerFactory
    {
        /// <summary>
        /// Trigger for lower level events to indidate that synchronization settings should be saved.
        /// Doesn't feel like this should be pushed up to the interface level.
        /// </summary>
        private AutoResetEvent _saveSynchronizationConfigurationsEvent;

        public CalendarSynchronizationWorkerFactory(AutoResetEvent saveSynchronizationConfigurationsEvent)
        {
            _saveSynchronizationConfigurationsEvent = saveSynchronizationConfigurationsEvent;
        }
        
        /// <summary>
        /// Given a synchronization config, return the sync worker.
        /// </summary>
        /// <param name="synchronizationConfiguration"></param>
        /// <param name="calendarConfigurations"></param>
        /// <param name="serviceConfigurations"></param>
        /// <returns></returns>
        public ICalendarSynchronizationWorker GetSynchronizationWorker(
            ISynchronizationConfiguration synchronizationConfiguration,
            IDictionary<Guid, ICalendarConfiguration> calendarConfigurations,
            IDictionary<CalendarType, IServiceConfiguration> serviceConfigurations)
        {
            var sourceCalendarConfig = calendarConfigurations[synchronizationConfiguration.SourceCalendarConfigurationId];
            var sourceServiceConfig = (serviceConfigurations.ContainsKey(sourceCalendarConfig.CalendarType)) ? serviceConfigurations[sourceCalendarConfig.CalendarType] : null;
            var sourceCalendarFactory = GetCalendarFactory(sourceCalendarConfig, sourceServiceConfig);
            var sourceCalendar = sourceCalendarFactory.GetCalendar();

            var destinationCalendarConfig =
                calendarConfigurations[synchronizationConfiguration.DestinationCalendarConfigurationId];
            var destinationServiceConfig = (serviceConfigurations.ContainsKey(destinationCalendarConfig.CalendarType)) ? serviceConfigurations[destinationCalendarConfig.CalendarType] : null;
            var destinationCalendarFactory = GetCalendarFactory(destinationCalendarConfig, destinationServiceConfig);
            var destinationCalendar = destinationCalendarFactory.GetCalendar();

            return new CalendarSynchronizationWorker(sourceCalendar, destinationCalendar,
                                                     synchronizationConfiguration, _saveSynchronizationConfigurationsEvent);
        }

        /// <summary>
        /// Helper method to get a calendar factory, consider splitting this out into yet another factory class.
        /// </summary>
        /// <param name="calendarConfiguration"></param>
        /// <param name="serviceConfiguration"></param>
        /// <returns></returns>
        private ICalendarFactory GetCalendarFactory(ICalendarConfiguration calendarConfiguration, IServiceConfiguration serviceConfiguration)
        {
            switch (calendarConfiguration.CalendarType)
            {
                case CalendarType.Google:
                    return GetGoogleCalendarFactory(calendarConfiguration, serviceConfiguration);
                case CalendarType.Exchange:
                    return GetExchangeCalendarFactory(calendarConfiguration);
                default:
                    throw new InvalidDataException("Unknown calendar type");
            }
        }

        /// <summary>
        /// Helper method to create an ExchangeCalendarFactory
        /// </summary>
        /// <param name="calendarConfiguration"></param>
        /// <returns></returns>
        private ICalendarFactory GetExchangeCalendarFactory(ICalendarConfiguration calendarConfiguration)
        {
            var exchangeCalendarConfiguration = calendarConfiguration as ExchangeCalendarConfiguration;
            if (exchangeCalendarConfiguration == null)
            {
                throw new InvalidDataException("Unable to cast calendar config to Exchange calendar format.");
            }

            return ExchangeCalendarFactory.CreateFromCalendarConfiguration(exchangeCalendarConfiguration);
        }

        /// <summary>
        /// Helper method to create a GoogleCalendarFactory
        /// </summary>
        /// <param name="calendarConfiguration"></param>
        /// <param name="serviceConfiguration"></param>
        /// <returns></returns>
        private ICalendarFactory GetGoogleCalendarFactory(ICalendarConfiguration calendarConfiguration, IServiceConfiguration serviceConfiguration)
        {
            var googleCalendarConfiguration = calendarConfiguration as GoogleCalendarConfiguration;
            if (googleCalendarConfiguration == null)
            {
                throw new InvalidDataException("Unable to cast calendar config to Google calendar format.");
            }

            var googleServiceConfiguration = serviceConfiguration as GoogleServiceConfiguration;
            if (googleServiceConfiguration == null)
            {
                throw new InvalidDataException("Unable to cast service config to Google service format.");
            }

            return new GoogleCalendarFactory(googleCalendarConfiguration, googleServiceConfiguration);
        }
    }
}
