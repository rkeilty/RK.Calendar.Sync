namespace RK.CalendarSync.Core
{
    public interface ICalendarSyncService
    {
        /// <summary>
        /// Starts the service and underlying synchronization
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the service and underlying synchronization
        /// </summary>
        void Stop();
    }
}