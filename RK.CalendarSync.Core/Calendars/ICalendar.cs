using System;
using System.Collections.Generic;
using RK.CalendarSync.Core.Calendars.Events;

namespace RK.CalendarSync.Core.Calendars
{
    internal interface ICalendar
    {
        /// <summary>
        /// Returns a list of events given the start and end time, inclusive.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        IEnumerable<ICalendarEvent> GetCalendarEvents(DateTimeOffset startDate, DateTimeOffset endDate);

        /// <summary>
        /// Given a list of calendar events, syncronizes the events marked as dirty.
        /// </summary>
        /// <param name="calendarEvents"></param>
        /// <returns>Whether the sync was successful</returns>
        bool SynchronizeDirtyEvents(IEnumerable<ICalendarEvent> calendarEvents);
    }
}
