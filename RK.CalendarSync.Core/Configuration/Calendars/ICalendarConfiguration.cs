using System;

namespace RK.CalendarSync.Core.Configuration.Calendars
{
    public interface ICalendarConfiguration
    {
        /// <summary>
        /// Unique configuration ID, used when determining what configurations to sync
        /// </summary>
        Guid CalendarConfigurationId { get; set; }

        /// <summary>
        /// The calendar type being used
        /// </summary>
        CalendarType CalendarType { get; }
    }
}