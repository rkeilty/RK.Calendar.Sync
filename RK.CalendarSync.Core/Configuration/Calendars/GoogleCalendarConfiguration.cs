using System;

namespace RK.CalendarSync.Core.Configuration.Calendars
{
    /// <summary>
    /// Contains configuration data for connecting to a Google calendar
    /// </summary>
    public class GoogleCalendarConfiguration : ICalendarConfiguration
    {
        /// <summary>
        /// Full email/username for connecting (john.doe@gmail.com)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The unique calendar ID
        /// </summary>
        public string CalendarId { get; set; }

        /// <summary>
        /// Unique configuration ID, used when determining what configurations to sync
        /// </summary>
        public Guid CalendarConfigurationId { get; set; }

        /// <summary>
        /// Calendar type as required by ICalendarConfiguration
        /// </summary>
        public CalendarType CalendarType { get { return CalendarType.Google; } }

        public GoogleCalendarConfiguration()
        { }

        public GoogleCalendarConfiguration(string userName,
                                                  string calendarId)
        {
            UserName = userName;
            CalendarId = calendarId;
        }
    }
}
