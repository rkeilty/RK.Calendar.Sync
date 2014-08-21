using System.Collections.Generic;
using Google.Apis.Calendar.v3;

namespace RK.CalendarSync.Core.Calendars.Services.Google
{
    /// <summary>
    /// Wrapper around the Google Calendar Service, to make unit testing easier.
    /// </summary>
    interface IGoogleCalendarService
    {
        string Version { get; }

        AclResource Acl { get; }
        string BasePath { get; }
        string BaseUri { get; }
        CalendarListResource CalendarList { get; }
        CalendarsResource Calendars { get; }
        ChannelsResource Channels { get; }
        ColorsResource Colors { get; }
        EventsResource Events { get; }
        IList<string> Features { get; }
        FreebusyResource Freebusy { get; }
        string Name { get; }
        SettingsResource Settings { get; }
    }
}
