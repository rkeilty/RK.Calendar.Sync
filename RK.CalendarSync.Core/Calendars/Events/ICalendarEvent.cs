using System;
using System.Collections.Generic;
using RK.CalendarSync.Core.Calendars.Events.Data;

namespace RK.CalendarSync.Core.Calendars.Events
{
    /// <summary>
    /// Interface for storing calendar event parameters
    /// Some interesting notes:
    /// Everything should be in DateTimeOffset, and there should NOT be a universal TimeZone across the event.  This wrecks havok with DST
    /// events if we have a master TimeZone.
    /// </summary>
    internal interface ICalendarEvent
    {
        /// <summary>
        /// Replaces all required attendees in the current event
        /// </summary>
        void ReplaceRequiredAttendees(IList<CalendarEventAttendee> attendees);

        /// <summary>
        /// Replaces all optional attendees in the current event
        /// </summary>
        void ReplaceOptionalAttendees(IList<CalendarEventAttendee> attendees);

        /// <summary>
        /// Attendees of the current event
        /// </summary>
        IList<CalendarEventAttendee> RequiredAttendees { get; }

        /// <summary>
        /// Attendees of the current event
        /// </summary>
        IList<CalendarEventAttendee> OptionalAttendees { get; }

        /// <summary>
        /// When the event was created in the calendar.
        /// </summary>
        DateTimeOffset Created { get; }

        /// <summary>
        /// Event marked for new creation on sync
        /// </summary>
        bool CreateOnSync { get; set; }

        /// <summary>
        /// Event marked for deletion on sync
        /// </summary>
        bool DeleteOnSync { get; set; }

        /// <summary>
        /// Description of the event
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// End of the event (if one exists)
        /// </summary>
        DateTimeOffset? End { get; set; }

        /// <summary>
        /// The event ID specific to the application domain (Google, Exchange, etc)
        /// </summary>
        string DomainSpecificEventId { get; set; }

        /// <summary>
        /// Has the event been marked as deleted on the source side
        /// </summary>
        bool IsDeleted { get; set; }

        /// <summary>
        /// Is this an all day long event?
        /// </summary>
        bool IsAllDayEvent { get; set; }

        /// <summary>
        /// Determines if values on this have been modified since it was created.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Determines if the current event is part of a recurring series.
        /// </summary>
        bool IsRecurring { get; }

        /// <summary>
        /// Location of the current event
        /// </summary>
        string Location { get; set; }

        /// <summary>
        /// The modification date of the event.
        /// </summary>
        DateTimeOffset Modified { get; }

        /// <summary>
        /// Organizer of the current event
        /// </summary>
        CalendarEventAttendee Organizer { get; set; }

        /// <summary>
        /// The recurrence ID of the event, used only for recurring events.  In conjunction with a UID, uniquely
        /// identifies a specific recurring instance.  Per iCal standards, this is generally a uniquely identifiable
        /// DateTime.
        /// </summary>
        DateTimeOffset? RecurrenceId { get; set; }

        /// <summary>
        /// iCal sequence value for recurring events
        /// </summary>
        int Sequence { get; set; }

        /// <summary>
        /// Start time of the event
        /// </summary>
        DateTimeOffset Start { get; set; }

        /// <summary>
        /// Subject of the current event
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// UID of the event, not unique for multiple instances of a recurring event, use in conjunction with <see cref="RecurrenceId"/>
        /// </summary>
        string UID { get; set; }

        /// <summary>
        /// Event marked for undeletion on sync
        /// </summary>
        bool UnDeleteOnSync { get; set; }
    }
}