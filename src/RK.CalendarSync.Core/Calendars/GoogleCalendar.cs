using System;
using System.Collections.Generic;
using System.Threading;
using Google;
using Google.Apis.Calendar.v3.Data;
using NLog;
using RK.CalendarSync.Core.Calendars.Events;
using RK.CalendarSync.Core.Calendars.Services.Google;
using RK.CalendarSync.Core.Calendars.Transformers;
using RK.CalendarSync.Core.Common;

namespace RK.CalendarSync.Core.Calendars
{
    internal class GoogleCalendar : ICalendar
    {
        /// <summary>
        /// Logger for the class
        /// </summary>
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        private readonly IGoogleCalendarService _calendarService;
        private readonly string _calendarId;
        private readonly TimeZoneInfo _defaultCalendarTimeZone;
        private readonly ICalendarEventTransformer<Event> _calendarEventTransformer;

        /// <summary>
        /// Google seems to have some throttling issues, they don't really like anything faster than 5 requests/second/user.
        /// </summary>
        private static readonly int MILLISECONDS_BETWEEN_UPDATE_REQUESTS = 200;

        /// <summary>
        /// Constructor needs access to the calendar service object, and calendar ID, it will be accessing.
        /// </summary>
        /// <param name="calendarService"></param>
        /// <param name="calendarId"></param>
        public GoogleCalendar(IGoogleCalendarService calendarService, string calendarId)
        {
            _calendarService = calendarService;
            _calendarId = calendarId;
            var calendar = _calendarService.Calendars.Get(_calendarId).Execute();
            var calendarOlsonTimeZone = calendar.TimeZone;
            _defaultCalendarTimeZone = OlsonTimeZone.OlsonTimeZoneToTimeZoneInfo(calendarOlsonTimeZone);

            _calendarEventTransformer = new GoogleCalendarEventTransformer(_defaultCalendarTimeZone);
        }


        /// <summary>
        /// Returns a list of events given the start and end time, inclusive.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IEnumerable<ICalendarEvent> GetCalendarEvents(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            var calendarEventsService = _calendarService.Events.List(_calendarId);
            
            // Given the calendar time zone we're syncing to, determine how far backward and forward in time to go (approximately)
            calendarEventsService.TimeMin = startDate.ToOffset(_defaultCalendarTimeZone.BaseUtcOffset).DateTime;
            calendarEventsService.TimeMax = endDate.ToOffset(_defaultCalendarTimeZone.BaseUtcOffset).DateTime;

            // Get recurring events as single events (so we don't have to rely on the "master" recurring event)
            calendarEventsService.SingleEvents = true;

            // Get "deleted" events, since Google only performs soft deletes, so that we can "undelete" if necessary.
            calendarEventsService.ShowDeleted = true;

            // Execute the command
            var calendarEvents = calendarEventsService.Execute();

            var returnEvents = new List<ICalendarEvent>();

            // Iterate
            foreach (var calendarEvent in calendarEvents.Items)
            {
                var returnEvent = _calendarEventTransformer.ConvertToCalendarEvent(calendarEvent);
                returnEvents.Add(returnEvent);
            }

            return returnEvents;
        }

        /// <summary>
        /// Given a list of calendar events, syncronizes the events marked as dirty.
        /// </summary>
        /// <param name="calendarEvents"></param>
        public void SynchronizeDirtyEvents(IEnumerable<ICalendarEvent> calendarEvents)
        {
            foreach (var calendarEvent in calendarEvents)
            {
                if (calendarEvent.IsDirty)
                {
                    Thread.Sleep(MILLISECONDS_BETWEEN_UPDATE_REQUESTS);

                    var googleCalendarEvent = _calendarEventTransformer.ConvertFromCalendarEvent(calendarEvent);

                    // Easiest action is to first see if it's marked as deleted
                    if (calendarEvent.DeleteOnSync)
                    {
                        DeleteCalendarEvent(googleCalendarEvent);
                        continue;
                    }

                    // Now see if we've marked for undeletion
                    if (calendarEvent.UnDeleteOnSync)
                    {
                        UndeleteCalendarEvent(googleCalendarEvent);
                        continue;
                    }

                    // Now see if it's new
                    if (calendarEvent.CreateOnSync)
                    {
                        CreateNewCalendarEvent(googleCalendarEvent);
                        continue;
                    }

                    // Otherwise, assume an update
                    UpdateCalendarEvent(googleCalendarEvent);
                }
            }

        }

        /// <summary>
        /// Helper method to delete.
        /// </summary>
        /// <param name="calendarEvent"></param>
        private void DeleteCalendarEvent(Event calendarEvent)
        {
            _calendarService.Events.Delete(_calendarId, calendarEvent.Id).Execute();
        }

        /// <summary>
        /// Helper method to undelete.
        /// </summary>
        /// <param name="calendarEvent"></param>
        private void UndeleteCalendarEvent(Event calendarEvent)
        {
            calendarEvent.Status = "confirmed"; // Default status for events, we'll use this for now
            UpdateCalendarEvent(calendarEvent);
        }

        /// <summary>
        /// Helper method to update.
        /// </summary>
        /// <param name="calendarEvent"></param>
        private void UpdateCalendarEvent(Event calendarEvent)
        {
            _calendarService.Events.Update(calendarEvent, _calendarId, calendarEvent.Id).Execute();
        }

        /// <summary>
        /// Helper method to create.
        /// </summary>
        /// <param name="calendarEvent"></param>
        private void CreateNewCalendarEvent(Event calendarEvent)
        {
            _calendarService.Events.Insert(calendarEvent, _calendarId).Execute();
        }
    }
}
