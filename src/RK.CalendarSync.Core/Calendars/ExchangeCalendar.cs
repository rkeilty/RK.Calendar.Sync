using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Exchange.WebServices.Data;
using RK.CalendarSync.Core.Calendars.Events;
using RK.CalendarSync.Core.Calendars.Transformers;

namespace RK.CalendarSync.Core.Calendars
{
    internal class ExchangeCalendar : ICalendar
    {
        private readonly ExchangeService _exchangeService;
        private readonly ICalendarEventTransformer<Appointment> _calendarEventTransformer;
        private const int MAX_EVENTS_TO_RETRIEVE = 2500;

        /// <summary>
        /// Exchange needs to know exactly what properties to retrieve outside of "FirstClassProperties"
        /// and this defines them.
        /// </summary>
        private readonly PropertyDefinition[] NonFirstClassAppointmentProperties = new[]
            {
                AppointmentSchema.AppointmentSequenceNumber,
                AppointmentSchema.AppointmentType,
                AppointmentSchema.Body,
                AppointmentSchema.DateTimeCreated,
                AppointmentSchema.End,
                AppointmentSchema.EndTimeZone,
                AppointmentSchema.ICalUid,
                AppointmentSchema.ICalRecurrenceId,
                AppointmentSchema.IsAllDayEvent,
                AppointmentSchema.IsRecurring,
                AppointmentSchema.LastModifiedTime,
                AppointmentSchema.Location,
                AppointmentSchema.OptionalAttendees,
                AppointmentSchema.Organizer,
                AppointmentSchema.Recurrence,
                AppointmentSchema.RequiredAttendees,
                AppointmentSchema.Start,
                AppointmentSchema.StartTimeZone,
                AppointmentSchema.Subject,
                AppointmentSchema.TimeZone
            };

        public ExchangeCalendar(ExchangeService exchangeService)
        {
            _exchangeService = exchangeService;
            _calendarEventTransformer = new ExchangeCalendarEventTransformer(_exchangeService);
        }

        /// <summary>
        /// Returns a list of events given the start and end time, inclusive.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public IEnumerable<ICalendarEvent> GetCalendarEvents(DateTimeOffset startDate, DateTimeOffset endDate)
        {
            // Initialize the calendar folder object with only the folder ID.
            var propertiesToGet = new PropertySet(PropertySet.IdOnly);
            propertiesToGet.RequestedBodyType = BodyType.Text;

            var calendar = CalendarFolder.Bind(_exchangeService, WellKnownFolderName.Calendar, propertiesToGet);
            
            // Set the start and end time and number of appointments to retrieve.
            var calendarView = new CalendarView(
                startDate.ToOffset(_exchangeService.TimeZone.BaseUtcOffset).DateTime,
                endDate.ToOffset(_exchangeService.TimeZone.BaseUtcOffset).DateTime,
                MAX_EVENTS_TO_RETRIEVE);

            // Retrieve a collection of appointments by using the calendar view.
            var appointments = calendar.FindAppointments(calendarView);

            // Get specific properties.
            var appointmentSpecificPropertiesToGet = new PropertySet(PropertySet.FirstClassProperties);
            appointmentSpecificPropertiesToGet.AddRange(NonFirstClassAppointmentProperties);
            appointmentSpecificPropertiesToGet.RequestedBodyType = BodyType.Text;
            _exchangeService.LoadPropertiesForItems(appointments, appointmentSpecificPropertiesToGet);

            return TransformExchangeAppointmentsToGenericEvents(appointments);
        }

        /// <summary>
        /// Given a list of calendar events, syncronizes the events marked as dirty.
        /// </summary>
        /// <param name="calendarEvents"></param>
        public void SynchronizeDirtyEvents(IEnumerable<ICalendarEvent> calendarEvents)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Small helper method for readability.
        /// </summary>
        /// <param name="exchangeAppointments"></param>
        /// <returns></returns>
        private IEnumerable<ICalendarEvent> TransformExchangeAppointmentsToGenericEvents(
            IEnumerable<Appointment> exchangeAppointments)
        {
            return exchangeAppointments.Select(appointment => _calendarEventTransformer.ConvertToCalendarEvent(appointment)).ToList();
        }
    }
}
