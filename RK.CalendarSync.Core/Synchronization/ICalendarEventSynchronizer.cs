using System;
using System.Collections.Generic;
using RK.CalendarSync.Core.Calendars.Events;

namespace RK.CalendarSync.Core.Synchronization
{
    internal interface ICalendarEventSynchronizer
    {
        /// <summary>
        /// Synchronize two event lists.
        /// </summary>
        /// <param name="lastSuccessfulSync"></param>
        /// <param name="lastSyncAheadDate"></param>
        /// <param name="sourceEventList"></param>
        /// <param name="destinationEventList"></param>
        /// <returns></returns>
        SynchronizedEventLists SynchronizeEventLists(
            DateTimeOffset lastSuccessfulSync,
            DateTimeOffset lastSyncAheadDate,
            IEnumerable<ICalendarEvent> sourceEventList, 
            IEnumerable<ICalendarEvent> destinationEventList);
    }
}