using System;
using System.Collections.Generic;

namespace RK.CalendarSync.Core.Configuration.Calendars
{
    public interface ICalendarConfigurationRetriever
    {
        /// <summary>
        /// Retrieves all of the calendar configurations keyed on their unique config ID
        /// </summary>
        /// <returns></returns>
        IDictionary<Guid, ICalendarConfiguration> GetCalendarConfigurations();
    }
}