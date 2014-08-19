using System;
using System.Collections.Generic;
using System.Linq;
using Google.Apis.Calendar.v3.Data;
using RK.CalendarSync.Core.Calendars.Events;
using RK.CalendarSync.Core.Calendars.Events.Data;

namespace RK.CalendarSync.Core.Calendars.Transformers
{
    /// <summary>
    /// Transforms between Google Calendar <see cref="Event"/> object into our <see cref="CalendarEvent"/> object.
    /// </summary>
    internal class GoogleCalendarEventTransformer : ICalendarEventTransformer<Event>
    {
        /// <summary>
        /// The formatting string used by DateTimeOffset.ToString to conform to RFC 3339 standards.
        /// </summary>
        private static readonly string ALL_DAY_EVENT_DATEFORMAT_STRING = "yyyy-MM-dd";
        private static readonly string RFC_3339_DATEFORMAT_STRING = "yyyy-MM-dd'T'HH:mm:ss.fffK";

        private readonly TimeZoneInfo _defaultTimeZone;
        
        /// <summary>
        /// Default timezone to use for the calendar.
        /// </summary>
        /// <param name="defaultTimeZone"></param>
        public GoogleCalendarEventTransformer(TimeZoneInfo defaultTimeZone)
        {
            _defaultTimeZone = defaultTimeZone;
        }

        public ICalendarEvent ConvertToCalendarEvent(Event googleCalendarEvent)
        {
            // Created and modified can be pulled from the raw values.
            var created = DateTimeOffset.Parse(googleCalendarEvent.CreatedRaw);
            var modified = DateTimeOffset.Parse(googleCalendarEvent.UpdatedRaw);
            
            // Pull the start and end times.
            var start = GetDateTimeOffsetFromEventDateTime(googleCalendarEvent.Start);
            var end = new DateTimeOffset?();
            if (!googleCalendarEvent.EndTimeUnspecified.GetValueOrDefault(true))
            {
                end = GetDateTimeOffsetFromEventDateTime(googleCalendarEvent.End);
            }

            // Other common entries.
            bool isAllDayEvent = (googleCalendarEvent.Start.Date != null);

            // Google is interesting with its "owner" field.  Sometimes the owner can be the calendar itself, in which case
            // it probably makes the most sense to resort to the creator.
            var organizer = googleCalendarEvent.Organizer.Self.GetValueOrDefault(false)
                                ? new CalendarEventAttendee(googleCalendarEvent.Creator.DisplayName,
                                                            googleCalendarEvent.Creator.Email)
                                : new CalendarEventAttendee(googleCalendarEvent.Organizer.DisplayName,
                                                            googleCalendarEvent.Organizer.Email);
            var requiredAttendees = GetAttendeeListFromEventAttendees(googleCalendarEvent.Attendees, true);
            var optionalAttendees = GetAttendeeListFromEventAttendees(googleCalendarEvent.Attendees, false);

            // If the status of the event is "cancelled", then this means the event was actually soft deleted.
            var isDeleted = googleCalendarEvent.Status.Equals("cancelled", StringComparison.CurrentCultureIgnoreCase);
            
            CalendarEvent calendarEvent;

            // As long as there isn't a specific identifier then this is recurring
            if (string.IsNullOrEmpty(googleCalendarEvent.RecurringEventId))
            {
                calendarEvent = CalendarEvent.CreateNonRecurringEvent(created, modified,
                                                                      googleCalendarEvent.Sequence.GetValueOrDefault(0),
                                                                      googleCalendarEvent.Summary,
                                                                      googleCalendarEvent.Description, start, end,
                                                                      isAllDayEvent,
                                                                      organizer, requiredAttendees, optionalAttendees,
                                                                      googleCalendarEvent.Location, isDeleted,
                                                                      googleCalendarEvent.ICalUID,
                                                                      googleCalendarEvent.Id);
            }
            else
            {
                // Most implementations of iCal happen to use the events initial start time as the "nonce" for recurrence ID.
                // This generally doesn't change even if the event itself changes.
                var recurrenceId = DateTimeOffset.Parse(googleCalendarEvent.OriginalStartTime.DateTimeRaw);
                calendarEvent = CalendarEvent.CreateRecurringEvent(created, modified,
                                                                   googleCalendarEvent.Sequence.GetValueOrDefault(0),
                                                                   googleCalendarEvent.Summary,
                                                                   googleCalendarEvent.Description, start, end,
                                                                   isAllDayEvent, organizer, requiredAttendees,
                                                                   optionalAttendees,
                                                                   googleCalendarEvent.Location, isDeleted,
                                                                   googleCalendarEvent.ICalUID,
                                                                   recurrenceId, googleCalendarEvent.Id);

            }

            return calendarEvent;
        }

        public Event ConvertFromCalendarEvent(ICalendarEvent calendarEvent)
        {
            // Set the start and end times
            var googleCalendarEvent = new Event();

            // Set start/end times based on all day event flag, Google APIs are very specific about what Date or DateTime fields you set
            // based on AllDayEvents.
            googleCalendarEvent.Start = GetEventDateTimeFromDateTimeOffset(calendarEvent.Start,
                                                                           calendarEvent.IsAllDayEvent);
            googleCalendarEvent.End = GetEventDateTimeFromDateTimeOffset(calendarEvent.End,
                                                                           calendarEvent.IsAllDayEvent);

            // Sset subject/description/location
            googleCalendarEvent.Summary = calendarEvent.Subject;
            googleCalendarEvent.Description = calendarEvent.Description;
            googleCalendarEvent.Location = calendarEvent.Location;
            
            // Set organizers and attendees
            googleCalendarEvent.Organizer = new Event.OrganizerData()
                {
                    DisplayName = calendarEvent.Organizer.DisplayName,
                    Email = calendarEvent.Organizer.Email
                };
            
            var googleRequiredAttendees =
                calendarEvent.RequiredAttendees.Select(
                    a => new EventAttendee() { DisplayName = a.DisplayName, Email = a.Email, Optional = false }).ToList();

            var googleOptionalAttendees =
                calendarEvent.OptionalAttendees.Select(
                    a => new EventAttendee() { DisplayName = a.DisplayName, Email = a.Email, Optional = true }).ToList();

            googleCalendarEvent.Attendees = googleRequiredAttendees.Union(googleOptionalAttendees).ToList();

            // If our object has a domain specific ID, set it.
            if (!string.IsNullOrEmpty(calendarEvent.DomainSpecificEventId))
            {
                googleCalendarEvent.Id = calendarEvent.DomainSpecificEventId;
            }

            // Set UID/Recurrence/Sequence
            googleCalendarEvent.ICalUID = calendarEvent.UID;
            googleCalendarEvent.Sequence = calendarEvent.Sequence;
            if (calendarEvent.RecurrenceId.HasValue)
            {
                // For now we're overloading the original start time with the recurrence
                // Recall we're using DateTimeOffset for recurrence
                googleCalendarEvent.OriginalStartTime = new EventDateTime()
                    {
                        DateTimeRaw = calendarEvent.RecurrenceId.Value.ToString(RFC_3339_DATEFORMAT_STRING)
                    };
            }

            return googleCalendarEvent;
        }

        /// <summary>
        /// Helper method to return a correctly formatted and parsed DateTime offset from Googles EventDateTime object
        /// </summary>
        /// <param name="googleEventDateTime"></param>
        /// <returns></returns>
        private DateTimeOffset GetDateTimeOffsetFromEventDateTime(EventDateTime googleEventDateTime)
        {
            if (string.IsNullOrEmpty(googleEventDateTime.Date))
            {
                return DateTimeOffset.Parse(googleEventDateTime.DateTimeRaw);
            }

            return GetDateTimeOffsetFromSimpleAllDayDate(googleEventDateTime.Date);
        }

        /// <summary>
        /// Given a basic "day", determine what the DateTimeOffset should be based on the calendar TimeZone
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTimeOffset GetDateTimeOffsetFromSimpleAllDayDate(string date)
        {
            var parsedDate = DateTime.Parse(date);

            // This is kind of insane, we need to check for DST on top of the normal time... wouldn't it be easier just to
            // pass me the raw RFC 3339 timestamp in the response...  Sadly this is due to pulling the TimeZone from the 
            // top level calendar which is simple Olson time, it wouldn't know about individual events.
            var additionalDstOffset = new TimeSpan(0, 0, 0, 0);
            if (_defaultTimeZone.SupportsDaylightSavingTime
                && _defaultTimeZone.IsDaylightSavingTime(parsedDate))
            {
                additionalDstOffset = new TimeSpan(0, 1, 0, 0);
            }

            return new DateTimeOffset(parsedDate, _defaultTimeZone.BaseUtcOffset.Add(additionalDstOffset));
        }

        /// <summary>
        /// Convert an EventAttendee list to a normalized calendar event attendee list.
        /// </summary>
        /// <param name="eventAttendees"></param>
        /// <returns></returns>
        private List<CalendarEventAttendee> GetAttendeeListFromEventAttendees(IList<EventAttendee> eventAttendees, bool requiredAttendees)
        {
            if (eventAttendees != null)
            {
                return eventAttendees.Where(a => a.Optional.GetValueOrDefault(false) == requiredAttendees)
                                     .Select(a => new CalendarEventAttendee(a.DisplayName, a.Email))
                                     .ToList();
            }

            return new List<CalendarEventAttendee>();
        }

        /// <summary>
        /// Given a DateTimeOffset and whether we're working with an all day event, craft the EventDateTime object for Googles API
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="isAllDayEvent"></param>
        /// <returns></returns>
        private EventDateTime GetEventDateTimeFromDateTimeOffset(DateTimeOffset? dateTimeOffset, bool isAllDayEvent)
        {
            var eventDateTime = new EventDateTime();

            // Set start/end times based on all day event flag, Google APIs are very specific about what Date or DateTime fields you set
            // based on AllDayEvents.
            if (isAllDayEvent)
            {
                if (dateTimeOffset.HasValue)
                {
                    eventDateTime.Date = dateTimeOffset.Value.ToString(ALL_DAY_EVENT_DATEFORMAT_STRING);
                }
            }
            else
            {
                if (dateTimeOffset.HasValue)
                {
                    // Must conform to RFC 3339 string per Google standards
                    eventDateTime.DateTimeRaw = dateTimeOffset.Value.ToString(RFC_3339_DATEFORMAT_STRING);
                }
            }

            return eventDateTime;
        }
    }
}