using RK.CalendarSync.Core.Configuration.Synchronization;

namespace RK.CalendarSync.Core
{
    internal interface ICalendarSynchronizationWorker
    {
        /// <summary>
        /// Signals the worker to start
        /// </summary>
        void Start();

        /// <summary>
        /// Signals the worker to stop
        /// </summary>
        void Stop();

        /// <summary>
        /// Retrieve the configuration for the current synchronization worker.
        /// </summary>
        ISynchronizationConfiguration SynchronizationConfiguration { get; }
    }
}
