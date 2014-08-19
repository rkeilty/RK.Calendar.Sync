using System;
using System.Collections.Generic;
using System.Linq;
using RK.CalendarSync.Core.Calendars.Events.Data;

namespace RK.CalendarSync.Core.Calendars.Events
{
    [Serializable]
    internal class CalendarEvent : ICalendarEvent
    {
        private CalendarEvent()
        {}

        /// <summary>
        /// Private constructor used by the <see cref="CreateNonRecurringEvent"/> method
        /// </summary>
        /// <param name="created"></param>
        /// <param name="modified"></param>
        /// <param name="sequence"></param>
        /// <param name="subject"></param>
        /// <param name="description"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isAllDayEvent"></param>
        /// <param name="organizer"></param>
        /// <param name="requiredAttendees"></param>
        /// <param name="optionalAttendees"></param>
        /// <param name="location"></param>
        /// <param name="isDeleted"></param>
        /// <param name="uid"></param>
        /// <param name="domainSpecificEventId"></param>
        private CalendarEvent(
            DateTimeOffset created,
            DateTimeOffset modified,
            int sequence,
            string subject,
            string description,
            DateTimeOffset start,
            DateTimeOffset? end,
            bool isAllDayEvent,
            CalendarEventAttendee organizer,
            List<CalendarEventAttendee> requiredAttendees,
            List<CalendarEventAttendee> optionalAttendees,
            string location,
            bool isDeleted,
            string uid,
            string domainSpecificEventId = null)
        {
            _isAllDayEvent = isAllDayEvent;
            Created = created;
            _domainSpecificEventId = domainSpecificEventId;
            Modified = modified;
            _sequence = sequence;
            _subject = subject;
            _description = description;
            _start = start;
            _end = end;
            _organizer = organizer;
            _requiredAttendees = requiredAttendees;
            _optionalAttendees = optionalAttendees;
            _location = location;
            _uid = uid;
            _isDeleted = isDeleted;

            // Mark the modified bits
            IsDirty = false;
            _createOnSync = false;
            _deleteOnSync = false;
            _unDeleteOnSync = false;
        }

        /// <summary>
        /// Private constructor used by the <see cref="CreateRecurringEvent"/> method
        /// </summary>
        /// <param name="created"></param>
        /// <param name="modified"></param>
        /// <param name="sequence"></param>
        /// <param name="subject"></param>
        /// <param name="description"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isAllDayEvent"></param>
        /// <param name="organizer"></param>
        /// <param name="requiredAttendees"></param>
        /// <param name="optionalAttendees"></param>
        /// <param name="location"></param>
        /// <param name="isDeleted"></param>
        /// <param name="uid"></param>
        /// <param name="recurrenceId"></param>
        /// <param name="domainSpecificEventId"></param>
        private CalendarEvent(
            DateTimeOffset created,
            DateTimeOffset modified,
            int sequence,
            string subject,
            string description,
            DateTimeOffset start,
            DateTimeOffset? end,
            bool isAllDayEvent,
            CalendarEventAttendee organizer,
            List<CalendarEventAttendee> requiredAttendees,
            List<CalendarEventAttendee> optionalAttendees,
            string location,
            bool isDeleted,
            string uid,
            DateTimeOffset? recurrenceId,
            string domainSpecificEventId = null) :
                this(created,
                     modified,
                     sequence,
                     subject,
                     description,
                     start,
                     end,
                     isAllDayEvent,
                     organizer,
                     requiredAttendees,
                     optionalAttendees,
                     location,
                     isDeleted,
                     uid,
                     domainSpecificEventId)
        {
            _recurrenceId = recurrenceId;
        }


        /// <summary>
        /// Creates an empty new event.
        /// </summary>
        /// <returns></returns>
        public static CalendarEvent CreateNewEmptyEvent()
        {
            var returnEvent = new CalendarEvent();
            returnEvent.CreateOnSync = true;
            return returnEvent;
        }


        /// <summary>
        /// Creates a non-dirty non-recurring event.
        /// </summary>
        /// <param name="created"></param>
        /// <param name="modified"></param>
        /// <param name="sequence"></param>
        /// <param name="subject"></param>
        /// <param name="description"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isAllDayEvent"></param>
        /// <param name="organizer"></param>
        /// <param name="requiredAttendees"></param>
        /// <param name="optionalAttendees"></param>
        /// <param name="location"></param>
        /// <param name="isDeleted"></param>
        /// <param name="uid"></param>
        /// <param name="domainSpecificEventId"></param>
        /// <returns></returns>
        public static CalendarEvent CreateNonRecurringEvent(
            DateTimeOffset created,
            DateTimeOffset modified,
            int sequence,
            string subject,
            string description,
            DateTimeOffset start,
            DateTimeOffset? end,
            bool isAllDayEvent,
            CalendarEventAttendee organizer,
            List<CalendarEventAttendee> requiredAttendees,
            List<CalendarEventAttendee> optionalAttendees,
            string location,
            bool isDeleted,
            string uid,
            string domainSpecificEventId = null)
        {
            return new CalendarEvent(
                created,
                modified,
                sequence,
                subject,
                description,
                start,
                end,
                isAllDayEvent,
                organizer,
                requiredAttendees,
                optionalAttendees,
                location,
                isDeleted,
                uid,
                domainSpecificEventId);
        }

        /// <summary>
        /// Creates a non-dirty recurring event.
        /// </summary>
        /// <param name="created"></param>
        /// <param name="modified"></param>
        /// <param name="sequence"></param>
        /// <param name="subject"></param>
        /// <param name="description"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="isAllDayEvent"></param>
        /// <param name="organizer"></param>
        /// <param name="requiredAttendees"></param>
        /// <param name="optionalAttendees"></param>
        /// <param name="location"></param>
        /// <param name="isDeleted"></param>
        /// <param name="uid"></param>
        /// <param name="recurrenceId"></param>
        /// <param name="domainSpecificEventId"></param>
        /// <returns></returns>
        public static CalendarEvent CreateRecurringEvent(
            DateTimeOffset created,
            DateTimeOffset modified,
            int sequence,
            string subject,
            string description,
            DateTimeOffset start,
            DateTimeOffset? end,
            bool isAllDayEvent,
            CalendarEventAttendee organizer,
            List<CalendarEventAttendee> requiredAttendees,
            List<CalendarEventAttendee> optionalAttendees,
            string location,
            bool isDeleted,
            string uid,
            DateTimeOffset? recurrenceId,
            string domainSpecificEventId = null)
        {
            return new CalendarEvent(
                created,
                modified,
                sequence,
                subject,
                description,
                start,
                end,
                isAllDayEvent,
                organizer,
                requiredAttendees,
                optionalAttendees,
                location,
                isDeleted,
                uid,
                recurrenceId,
                domainSpecificEventId);
        }

        /// <summary>
        /// Replaces all required attendees in an event
        /// </summary>
        public void ReplaceRequiredAttendees(IList<CalendarEventAttendee> attendees)
        {
            _requiredAttendees = attendees.Distinct().ToList();
            IsDirty = true;
        }

        /// <summary>
        /// Readonly list of required attendees, use setter methods to add/remove.
        /// </summary>
        public IList<CalendarEventAttendee> RequiredAttendees
        {
            get { return _requiredAttendees.AsReadOnly(); }
        }
        private List<CalendarEventAttendee> _requiredAttendees;

        /// <summary>
        /// Replaces all optional attendees in an event
        /// </summary>
        public void ReplaceOptionalAttendees(IList<CalendarEventAttendee> attendees)
        {
            _optionalAttendees = attendees.Distinct().ToList();
            IsDirty = true;
        }

        /// <summary>
        /// Readonly list of optional attendees, use setter methods to add/remove.
        /// </summary>
        public IList<CalendarEventAttendee> OptionalAttendees
        {
            get { return _optionalAttendees.AsReadOnly(); }
        }
        private List<CalendarEventAttendee> _optionalAttendees;

        /// <summary>
        /// When the event was created in the calendar.
        /// </summary>
        public DateTimeOffset Created { get; private set; }

        /// <summary>
        /// Event marked for new creation on sync
        /// </summary>
        public bool CreateOnSync
        {
            get { return _createOnSync; }
            set
            {
                IsDirty = IsDirty || !_createOnSync.Equals(value);
                _createOnSync = value;
            }
        }
        private bool _createOnSync;

        /// <summary>
        /// Event marked for deletion on sync
        /// </summary>
        public bool DeleteOnSync
        {
            get { return _deleteOnSync; }
            set
            {
                IsDirty = IsDirty || !_deleteOnSync.Equals(value);
                _deleteOnSync = value;
            }
        }
        private bool _deleteOnSync;


        /// <summary>
        /// Description of the event
        /// </summary>
        public string Description 
        {
            get { return _description; }
            set
            {
                IsDirty = IsDirty || _description == null || !_description.Equals(value);
                _description = value;
            }
        }
        private string _description;


        /// <summary>
        /// End of the event (if one exists)
        /// </summary>
        public DateTimeOffset? End
        {
            get { return _end; }
            set
            {
                IsDirty = IsDirty || !_end.Equals(value);
                _end = value;
            }
        }
        private DateTimeOffset? _end;


        /// <summary>
        /// The event ID specific to the application domain (Google, Exchange, etc)
        /// </summary>
        public string DomainSpecificEventId
        {
            get { return _domainSpecificEventId; }
            set
            {
                IsDirty = IsDirty || _domainSpecificEventId == null || !_domainSpecificEventId.Equals(value);
                _domainSpecificEventId = value;
            }
        }
        private string _domainSpecificEventId;


        /// <summary>
        /// Is this an all day long event?
        /// </summary>
        public bool IsAllDayEvent
        {
            get { return _isAllDayEvent; }
            set
            {
                IsDirty = IsDirty || !_isAllDayEvent.Equals(value);
                _isAllDayEvent = value;
            }
        }
        private bool _isAllDayEvent;


        /// <summary>
        /// Has the event been marked as deleted on the source side
        /// </summary>
        public bool IsDeleted
        {
            get { return _isDeleted; }
            set
            {
                IsDirty = IsDirty || !_isDeleted.Equals(value);
                _isDeleted = value;
            }
        }
        private bool _isDeleted;

        /// <summary>
        /// Determines if values on this have been modified since it was created.
        /// </summary>
        public bool IsDirty { get; private set; }


        /// <summary>
        /// Determines if the current event is part of a recurring series.
        /// </summary>
        public bool IsRecurring
        {
            get { return RecurrenceId.HasValue; }
        }

        /// <summary>
        /// Location of the event.
        /// </summary>
        public string Location
        {
            get { return _location; }
            set
            {
                IsDirty = IsDirty || _location == null || !_location.Equals(value);
                _location = value;
            }
        }
        private string _location;

        /// <summary>
        /// The modification date of the event.
        /// </summary>
        public DateTimeOffset Modified { get; private set; }

        /// <summary>
        /// Organizer of the event
        /// </summary>
        public CalendarEventAttendee Organizer
        {
            get { return _organizer; }
            set
            {
                IsDirty = IsDirty || _organizer == null || !_organizer.DisplayName.Equals(value.DisplayName) || !_organizer.Email.Equals(value.Email);
                _organizer = value;
            }
        }
        private CalendarEventAttendee _organizer;


        /// <summary>
        /// The recurrence ID of the event, used only for recurring events.  In conjunction with a UID, uniquely
        /// identifies a specific recurring instance.
        /// </summary>
        public DateTimeOffset? RecurrenceId
        {
            get { return _recurrenceId; }
            set
            {
                IsDirty = IsDirty || !_recurrenceId.Equals(value);
                _recurrenceId = value;
            }
        }
        private DateTimeOffset? _recurrenceId;


        /// <summary>
        /// iCal sequence value for recurring events
        /// </summary>
        public int Sequence
        {
            get { return _sequence; }
            set
            {
                IsDirty = IsDirty || _sequence != value;
                _sequence = value;
            }
        }
        private int _sequence;


        /// <summary>
        /// Start time of the event
        /// </summary>
        public DateTimeOffset Start
        {
            get { return _start; }
            set
            {
                IsDirty = IsDirty || !_start.Equals(value);
                _start = value;
            }
        }
        private DateTimeOffset _start;


        /// <summary>
        /// Subject of the current event
        /// </summary>
        public string Subject
        {
            get { return _subject; }
            set
            {
                IsDirty = IsDirty || _subject == null || !_subject.Equals(value);
                _subject = value;
            }
        }
        private string _subject;

        /// <summary>
        /// UID of the event, not unique for multiple instances of a recurring event, use in conjunction with <see cref="RecurrenceId"/>
        /// </summary>
        public string UID
        {
            get { return _uid; }
            set
            {
                IsDirty = IsDirty || _uid == null || !_uid.Equals(value);
                _uid = value;
            }
        }
        private string _uid;

        /// <summary>
        /// Event marked for undeletion on sync
        /// </summary>
        public bool UnDeleteOnSync
        {
            get { return _unDeleteOnSync; }
            set
            {
                IsDirty = IsDirty || !_unDeleteOnSync.Equals(value);
                _unDeleteOnSync = value;
            }
        }
        private bool _unDeleteOnSync;
    }
}
