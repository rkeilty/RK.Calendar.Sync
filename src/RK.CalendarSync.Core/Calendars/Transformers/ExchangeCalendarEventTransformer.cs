using System;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;
using RK.CalendarSync.Core.Calendars.Events;
using RK.CalendarSync.Core.Calendars.Events.Data;
using CalendarEvent = RK.CalendarSync.Core.Calendars.Events.CalendarEvent;

namespace RK.CalendarSync.Core.Calendars.Transformers
{
    /// <summary>
    /// Transforms between Exchange <see cref="Appointment"/> object into our <see cref="CalendarEvent"/> object.
    /// </summary>
    internal class ExchangeCalendarEventTransformer : ICalendarEventTransformer<Appointment>
    {
        /// <summary>
        /// Default time-zone for the calendar service.
        /// </summary>
        private readonly TimeZoneInfo _defaultTimeZone;

        /// <summary>
        /// Some of the underlying objects require the exchange service object directly (like the Appointment constructor)
        /// </summary>
        private readonly ExchangeService _exchangeService;

        public ExchangeCalendarEventTransformer(ExchangeService exchangeService)
        {
            _defaultTimeZone = exchangeService.TimeZone;
            _exchangeService = exchangeService;
        }

        public ICalendarEvent ConvertToCalendarEvent(Appointment exchangeCalendarEvent)
        {
            // EWS automatically ensures that created/modified/start/end have the appropriate DateTime.Kind set (not unknown.)
            // Error check this in the future, just to be safe.
            var created = new DateTimeOffset(exchangeCalendarEvent.DateTimeCreated);
            var modified = new DateTimeOffset(exchangeCalendarEvent.LastModifiedTime);
            var start = new DateTimeOffset(exchangeCalendarEvent.Start);
            var end = new DateTimeOffset(exchangeCalendarEvent.End);

            // Locations and attendees
            var organizer = new CalendarEventAttendee(exchangeCalendarEvent.Organizer.Name, exchangeCalendarEvent.Organizer.Address);
            var requiredAttendees =
                exchangeCalendarEvent.RequiredAttendees.Select(a => new CalendarEventAttendee(a.Name, a.Address))
                                     .ToList();
            var optionalAttendees =
                exchangeCalendarEvent.OptionalAttendees.Select(a => new CalendarEventAttendee(a.Name, a.Address))
                                     .ToList();

            CalendarEvent calendarEvent;

            // As long as there isn't a specific identifier then this is recurring
            if (!exchangeCalendarEvent.ICalRecurrenceId.HasValue)
            {

                calendarEvent = CalendarEvent.CreateNonRecurringEvent(created, modified,
                                                                      exchangeCalendarEvent.AppointmentSequenceNumber,
                                                                      exchangeCalendarEvent.Subject,
                                                                      exchangeCalendarEvent.Body.Text, start, end,
                                                                      exchangeCalendarEvent.IsAllDayEvent,
                                                                      organizer, requiredAttendees, optionalAttendees,
                                                                      exchangeCalendarEvent.Location,
                                                                      false,
                                                                      exchangeCalendarEvent.ICalUid,
                                                                      exchangeCalendarEvent.Id.UniqueId);
            }
            else
            {
                // Most implementations of iCal happen to use the events initial start time as the "nonce" for recurrence ID.
                // This generally doesn't change even if the event itself changes.
                var recurrenceId = new DateTimeOffset(exchangeCalendarEvent.ICalRecurrenceId.Value);

                calendarEvent = CalendarEvent.CreateRecurringEvent(created, modified,
                                                                   exchangeCalendarEvent.AppointmentSequenceNumber,
                                                                   exchangeCalendarEvent.Subject,
                                                                   exchangeCalendarEvent.Body.Text, start, end,
                                                                   exchangeCalendarEvent.IsAllDayEvent,
                                                                   organizer, requiredAttendees, optionalAttendees,
                                                                   exchangeCalendarEvent.Location,
                                                                   false,
                                                                   exchangeCalendarEvent.ICalUid, recurrenceId,
                                                                   exchangeCalendarEvent.Id.UniqueId);
            }

            return calendarEvent;
        }

        public Appointment ConvertFromCalendarEvent(ICalendarEvent calendarEvent)
        {
            throw new NotImplementedException();
        }
    }
}
