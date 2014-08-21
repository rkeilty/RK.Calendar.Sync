using RK.CalendarSync.Core.Calendars.Events;

namespace RK.CalendarSync.Core.Calendars.Transformers
{
    internal interface ICalendarEventTransformer<T>
    {
        ICalendarEvent ConvertToCalendarEvent(T calendarEvent);
        T ConvertFromCalendarEvent(ICalendarEvent calendarEvent);
    }
}
