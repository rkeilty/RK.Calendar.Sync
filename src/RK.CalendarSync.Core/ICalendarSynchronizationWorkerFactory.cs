using System;
using System.Collections.Generic;
using RK.CalendarSync.Core.Configuration.Calendars;
using RK.CalendarSync.Core.Configuration.Services;
using RK.CalendarSync.Core.Configuration.Synchronization;

namespace RK.CalendarSync.Core
{
    internal interface ICalendarSynchronizationWorkerFactory
    {
        /// <summary>
        /// Given a synchronization config, return the sync worker.
        /// </summary>
        /// <param name="synchronizationConfiguration"></param>
        /// <param name="calendarConfigurations"></param>
        /// <param name="serviceConfigurations"></param>
        /// <returns></returns>
        ICalendarSynchronizationWorker GetSynchronizationWorker(
            ISynchronizationConfiguration synchronizationConfiguration,
            IDictionary<Guid, ICalendarConfiguration> calendarConfigurations,
            IDictionary<CalendarType, IServiceConfiguration> serviceConfigurations);
    }
}