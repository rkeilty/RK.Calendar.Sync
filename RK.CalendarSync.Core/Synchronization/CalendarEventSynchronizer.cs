using System;
using System.Collections.Generic;
using System.Linq;
using RK.CalendarSync.Core.Calendars.Events;

namespace RK.CalendarSync.Core.Synchronization
{
    internal class CalendarEventSynchronizer : ICalendarEventSynchronizer
    {
        public CalendarEventSynchronizer()
        { }

        /// <summary>
        /// Synchronize two event lists.
        /// </summary>
        /// <param name="lastSuccessfulSync"></param>
        /// <param name="lastSyncAheadDate"></param>
        /// <param name="sourceEventList"></param>
        /// <param name="destinationEventList"></param>
        /// <returns></returns>
        public SynchronizedEventLists SynchronizeEventLists(
            DateTimeOffset lastSuccessfulSync,
            DateTimeOffset lastSyncAheadDate,
            IEnumerable<ICalendarEvent> sourceEventList, 
            IEnumerable<ICalendarEvent> destinationEventList)
        {
            // The main logical identifier between events is an iCal UID specifier.  We should take the time to create
            // a lookup on that parameter.

            var sourceEventDictionary =
                new Dictionary<CalendarEventSynchronizationKey, ICalendarEvent>(
                    sourceEventList.ToDictionary(
                        ce => new CalendarEventSynchronizationKey(ce.UID, ce.RecurrenceId),
                        ce => ce));

            var destinationEventDictionary =
                new Dictionary<CalendarEventSynchronizationKey, ICalendarEvent>(
                    destinationEventList.ToDictionary(
                        ce => new CalendarEventSynchronizationKey(ce.UID, ce.RecurrenceId),
                        ce => ce));

            var synchronizedKeysList = new SortedSet<CalendarEventSynchronizationKey>();

            // Synchronize all events, treat recurring events as single instances when performing a deep clone.
            // This is not great, ideally we would want to store the recurring information in some underlying schema, but
            // this gets the job done for an early version of the code.
            SynchronizeEvents(lastSuccessfulSync, lastSyncAheadDate, sourceEventDictionary, destinationEventDictionary, synchronizedKeysList);
            SynchronizeEvents(lastSuccessfulSync, lastSyncAheadDate, destinationEventDictionary, sourceEventDictionary, synchronizedKeysList);

            return new SynchronizedEventLists(sourceEventDictionary.Values, destinationEventDictionary.Values);
        }

        private void SynchronizeEvents(
            DateTimeOffset lastSuccessfulSync,
            DateTimeOffset lastSyncAheadDate,
            IDictionary<CalendarEventSynchronizationKey, ICalendarEvent> sourceEventDictionary,
            IDictionary<CalendarEventSynchronizationKey, ICalendarEvent> destinationEventDictionary,
            ISet<CalendarEventSynchronizationKey> synchronizedKeysList)
        {
            // First, go through event event in list 1, and see if it exists in list 2.
            foreach (var sourceEventEntry in sourceEventDictionary)
            {
                var sourceEventKey = sourceEventEntry.Key;
                var sourceEvent = sourceEventEntry.Value;

                synchronizedKeysList.Add(sourceEventKey);

                // If it exists in distination list, verify the appropriate fields are the same, if not, update to most recently modified version.
                if (destinationEventDictionary.ContainsKey(sourceEventKey))
                {
                    SynchronizeCalendarEvents(sourceEvent, destinationEventDictionary[sourceEventKey]);
                }
                else
                {
                    // If it doesn't exist in destination list, see if the items creation time is after the last sync, and also make sure
                    // that it's not marked as deleted (because then there is no need to sync with the second list.)
                    //
                    // We also look to see if the source event exists in one list, not the other, and has a start date more recent than
                    // the last sync ahead date (this way we continue to import new events.)  It may not have been created after the last
                    // sync (depending on sync look ahead days), but it probably starts after the last look ahead date.
                    if (!sourceEvent.IsDeleted)
                    {
                        if (sourceEvent.Created > lastSuccessfulSync || sourceEvent.Start > lastSyncAheadDate)
                        {
                            // If it is, create it in destination list, make a deep copy of the current event
                            var destinationEvent = CalendarEvent.CreateNewEmptyEvent();
                            CopyCommonCalendarEventProperties(sourceEvent, destinationEvent);

                            destinationEventDictionary.Add(
                                new CalendarEventSynchronizationKey(
                                    destinationEvent.UID, destinationEvent.RecurrenceId),
                                destinationEvent);
                        }
                        else
                        {
                            // If it isn't, then it means it was deleted from destination, so we should delete from source.
                            sourceEvent.DeleteOnSync = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Synchronize two events we know are the same, changes mark the underlying object as dirty
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="event2"></param>
        private void SynchronizeCalendarEvents(ICalendarEvent event1, ICalendarEvent event2)
        {
            // If either calendar is marked for deletion, we have a special case
            if (event1.IsDeleted || event2.IsDeleted)
            {
                SynchronizeDeletedCalendarEvents(event1, event2);
                return;
            }

            // Otherwise, update oldest common properties.
            UpdateOldestEventCommonProperties(event1, event2);
        }


        /// <summary>
        /// Helper function to update the older of the two events.
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="event2"></param>
        private void UpdateOldestEventCommonProperties(ICalendarEvent event1, ICalendarEvent event2)
        {
            // Otherwise, simply check the common properties and sync.
            if (AreCommonCalendarEventPropertiesEqual(event1, event2))
            {
                return;
            }

            if (event1.Modified > event2.Modified)
            {
                CopyCommonCalendarEventProperties(event1, event2);
            }
            else
            {
                CopyCommonCalendarEventProperties(event2, event1);
            }
        }


        /// <summary>
        /// Synchronize two events we know are the same, changes mark the underlying object as dirty
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="event2"></param>
        private void SynchronizeDeletedCalendarEvents(ICalendarEvent event1, ICalendarEvent event2)
        {
            // If both calendars are deleted, do nothing, we don't care.
            if (event1.IsDeleted && event2.IsDeleted)
            {
                return;
            }

            // If one of the calendars is marked as deleted, and the other is not, then
            //      - If the least recently modified calendar is not deleted, we should mark it for deletion on sync
            //      - If the least recently modified calendar is deleted, we should mark it for un-deletion on sync (and update it's properties.)
            
            if (event1.Modified > event2.Modified)
            {
                if (event1.IsDeleted)
                {
                    event2.DeleteOnSync = true;
                }
                else
                {
                    // If we're undeleting, update common properties.
                    event2.UnDeleteOnSync = true;
                    UpdateOldestEventCommonProperties(event1, event2);
                }
            }
            else
            {
                if (event2.IsDeleted)
                {
                    event1.DeleteOnSync = true;
                }
                else
                {
                    // If we're undeleting, update common properties.
                    event1.UnDeleteOnSync = true;
                    UpdateOldestEventCommonProperties(event1, event2);
                }
            }
        }


        /// <summary>
        /// Copy all common properties from one event to another.
        /// </summary>
        /// <param name="copyFrom"></param>
        /// <param name="copyTo"></param>
        private void CopyCommonCalendarEventProperties(ICalendarEvent copyFrom, ICalendarEvent copyTo)
        {
            // Remove and re-add attendees
            copyTo.ReplaceRequiredAttendees(copyFrom.RequiredAttendees);
            copyTo.ReplaceOptionalAttendees(copyFrom.OptionalAttendees);

            // Common properties
            copyTo.Description = copyFrom.Description;
            copyTo.End = copyFrom.End;
            copyTo.IsAllDayEvent = copyFrom.IsAllDayEvent;
            copyTo.Location = copyFrom.Location;
            copyTo.Organizer = copyFrom.Organizer;
            copyTo.RecurrenceId = copyFrom.RecurrenceId;
            copyTo.Start = copyFrom.Start;
            copyTo.Subject = copyFrom.Subject;
            copyTo.UID = copyFrom.UID;

            // Update the sequence number
            copyTo.Sequence++;
        }


        /// <summary>
        /// See if all of the common properties between two calendars are equal.
        /// </summary>
        /// <param name="calendarEvent1"></param>
        /// <param name="calendarEvent2"></param>
        private bool AreCommonCalendarEventPropertiesEqual(ICalendarEvent calendarEvent1, ICalendarEvent calendarEvent2)
        {
            return calendarEvent1.IsAllDayEvent == calendarEvent2.IsAllDayEvent
                   && calendarEvent1.RequiredAttendees.Except(calendarEvent2.RequiredAttendees).Any()
                   && calendarEvent2.RequiredAttendees.Except(calendarEvent1.RequiredAttendees).Any()
                   && calendarEvent1.OptionalAttendees.Except(calendarEvent2.OptionalAttendees).Any()
                   && calendarEvent2.OptionalAttendees.Except(calendarEvent1.OptionalAttendees).Any()
                   && (calendarEvent1.Description ?? string.Empty).Equals(calendarEvent2.Description ?? string.Empty)
                   && calendarEvent1.End.Equals(calendarEvent2.End)
                   && calendarEvent1.Location.Equals(calendarEvent2.Location)
                   && calendarEvent1.Organizer.Equals(calendarEvent2.Organizer)
                   && calendarEvent1.RecurrenceId.Equals(calendarEvent2.RecurrenceId)
                   && calendarEvent1.Start.Equals(calendarEvent2.Start)
                   && (calendarEvent1.Subject ?? string.Empty).Equals(calendarEvent2.Subject ?? string.Empty)
                   && calendarEvent1.UID.Equals(calendarEvent2.UID);
        }
    }
}
