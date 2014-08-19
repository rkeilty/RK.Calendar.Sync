using System.Collections.Generic;
using RK.CalendarSync.Core.Calendars.Events;

namespace RK.CalendarSync.Core.Synchronization
{
    internal class SynchronizedEventLists
    {
        public SynchronizedEventLists(IEnumerable<ICalendarEvent> sourceEventList, IEnumerable<ICalendarEvent> destinationEventList)
        {
            SourceEventList = sourceEventList;
            DestinationEventList = destinationEventList;
        }

        /// <summary>
        /// One of the synchronized lists.
        /// </summary>
        public IEnumerable<ICalendarEvent> SourceEventList { get; private set; }

        /// <summary>
        /// One of the synchronized lists.
        /// </summary>
        public IEnumerable<ICalendarEvent> DestinationEventList { get; private set; } 
    }
}
