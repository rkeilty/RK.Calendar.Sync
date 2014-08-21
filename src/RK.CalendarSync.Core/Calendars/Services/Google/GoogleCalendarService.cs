using System.Collections.Generic;
using Google.Apis.Calendar.v3;

namespace RK.CalendarSync.Core.Calendars.Services.Google
{
    /// <summary>
    /// Wrapper around the real Google Calendar Service class.
    /// </summary>
    internal class GoogleCalendarService : IGoogleCalendarService
    {
        private readonly CalendarService _calendarService;

        public GoogleCalendarService(CalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        public string Version { get { return CalendarService.Version; } }
        public AclResource Acl { get { return _calendarService.Acl; } }
        public string BasePath { get { return _calendarService.BasePath; } }
        public string BaseUri { get { return _calendarService.BaseUri; } }
        public CalendarListResource CalendarList { get { return _calendarService.CalendarList; } }
        public CalendarsResource Calendars { get { return _calendarService.Calendars; } }
        public ChannelsResource Channels { get { return _calendarService.Channels; } }
        public ColorsResource Colors { get { return _calendarService.Colors; } }
        public EventsResource Events { get { return _calendarService.Events; } }
        public IList<string> Features { get { return _calendarService.Features; } }
        public FreebusyResource Freebusy { get { return _calendarService.Freebusy; } }
        public string Name { get { return _calendarService.Name; } }
        public SettingsResource Settings { get { return _calendarService.Settings; } }
    }
}
