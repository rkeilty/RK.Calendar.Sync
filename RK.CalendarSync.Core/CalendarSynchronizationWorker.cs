using System;
using System.Linq;
using System.Threading;
using NLog;
using RK.CalendarSync.Core.Calendars;
using RK.CalendarSync.Core.Configuration.Synchronization;
using RK.CalendarSync.Core.Synchronization;

namespace RK.CalendarSync.Core
{
    internal class CalendarSynchronizationWorker : ICalendarSynchronizationWorker
    {
        /// <summary>
        /// Logger for the class
        /// </summary>
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// If we fail to sync, wait 10 minutes before retrying.
        /// </summary>
        private const int DEFAULT_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL = 600000;

        private readonly ICalendar _sourceCalendar;
        private readonly ICalendar _destinationCalendar;
        private readonly ISynchronizationConfiguration _synchronizationConfiguration;
        private readonly ICalendarEventSynchronizer _calendarEventSynchronizer;
        private readonly AutoResetEvent _syncWaitEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent _saveSynchronizationConfigurationsEvent;

        private bool _stopRequested = false;

        public CalendarSynchronizationWorker(
            ICalendar sourceCalendar,
            ICalendar destinationCalendar,
            ISynchronizationConfiguration synchronizationConfiguration,
            AutoResetEvent saveSynchronizationConfigurationsEvent)
            : this(
                sourceCalendar, destinationCalendar, synchronizationConfiguration, saveSynchronizationConfigurationsEvent,
                new CalendarEventSynchronizer())
        { }

        public CalendarSynchronizationWorker(
            ICalendar sourceCalendar, 
            ICalendar destinationCalendar,
            ISynchronizationConfiguration synchronizationConfiguration,
            AutoResetEvent saveSynchronizationConfigurationsEvent,
            ICalendarEventSynchronizer calendarEventSynchronizer)
        {
            _sourceCalendar = sourceCalendar;
            _destinationCalendar = destinationCalendar;
            _synchronizationConfiguration = synchronizationConfiguration;
            _saveSynchronizationConfigurationsEvent = saveSynchronizationConfigurationsEvent;
            _calendarEventSynchronizer = calendarEventSynchronizer;
        }


        /// <summary>
        /// Starts the current worker thread.
        /// </summary>
        public void Start()
        {
            var nextSyncTime =
                _synchronizationConfiguration.LastSynchronization.GetValueOrDefault(DateTimeOffset.MinValue)
                                             .AddMinutes(_synchronizationConfiguration.MinutesBetweenSynchronization);
            var millisecondsBeforeNextSync = (int)Math.Max(nextSyncTime.Subtract(DateTimeOffset.Now).TotalMilliseconds, 0);

            while (!_stopRequested)
            {
                _syncWaitEvent.WaitOne(millisecondsBeforeNextSync);

                if (_stopRequested)
                {
                    break;
                }

                // Log that we're synchronizing
                var infoLog = new LogEventInfo(LogLevel.Info, LOGGER.Name, "Attempting to synchronize calendar");
                infoLog.Properties.Add("SourceCalendarId", _synchronizationConfiguration.SourceCalendarConfigurationId);
                infoLog.Properties.Add("DestinationCalendarId", _synchronizationConfiguration.DestinationCalendarConfigurationId);
                LOGGER.Log(infoLog);

                // If the sync succeeds, determine how long to wait and write the success time back to disk.
                var attemptedSyncTime = DateTimeOffset.Now;
                if (SynchronizeAndUpdate())
                {
                    _synchronizationConfiguration.LastSynchronization = attemptedSyncTime;
                    
                    // Indicate to whoever cares that we should re-save the configuration settings.
                    _saveSynchronizationConfigurationsEvent.Set();

                    nextSyncTime =
                        _synchronizationConfiguration.LastSynchronization.GetValueOrDefault(DateTimeOffset.MinValue)
                                                     .AddMinutes(_synchronizationConfiguration.MinutesBetweenSynchronization);
                    millisecondsBeforeNextSync = (int)Math.Max(nextSyncTime.Subtract(DateTimeOffset.Now).TotalMilliseconds, 0);
                }
                else
                {
                    // If we failed, then simply wait a deteremined amount of time before retrying
                    millisecondsBeforeNextSync = DEFAULT_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL;
                }
            }
        }

        /// <summary>
        /// Stops the current worker thread.
        /// </summary>
        public void Stop()
        {
            // Indicate a shutdown event and trigger the sync wait to continue
            _stopRequested = true;
            _syncWaitEvent.Set();
        }

        /// <summary>
        /// Retrieve the configuration for the current synchronization worker.
        /// </summary>
        public ISynchronizationConfiguration SynchronizationConfiguration { get { return _synchronizationConfiguration; } }

        /// <summary>
        /// Synchronizes and updates event lists
        /// </summary>
        /// <returns>Was the syncronization successful</returns>
        private bool SynchronizeAndUpdate()
        {
            var syncEventStartDate = DateTimeOffset.Now.AddDays(0 - _synchronizationConfiguration.DaysInPastToSync);
            var syncEventEndDate = DateTimeOffset.Now.AddDays(_synchronizationConfiguration.DaysInFutureToSync);

            var sourceCalendarEvents = _sourceCalendar.GetCalendarEvents(syncEventStartDate, syncEventEndDate);
            if (_stopRequested)
            {
                return false;
            }
            var destinationCalendarEvents = _destinationCalendar.GetCalendarEvents(syncEventStartDate, syncEventEndDate);
            if (_stopRequested)
            {
                return false;
            }

            // Some tricky intelligence has to happen during the sync which I'm not a huge fan of, the main issue being
            // that we can't often do lookups by UID/RecurrenceId on API calls, so we can definitively take a look to see
            // if a given UID/RecurrenceId may have been moved outside the time-range we're looking at.  We'll try to 
            // be as smart as possible.

            // It doesn't matter what the sync direction is here, it only matters when we update the dirty event.
            var synchronizedEvents = _calendarEventSynchronizer.SynchronizeEventLists(
                _synchronizationConfiguration.LastSynchronization.GetValueOrDefault(DateTimeOffset.MinValue),
                _synchronizationConfiguration.LastSyncAheadDate.GetValueOrDefault(DateTimeOffset.MinValue),
                sourceCalendarEvents,
                destinationCalendarEvents);

            LogSynchronizedEvents(synchronizedEvents);

            if (_stopRequested)
            {
                return false;
            }

            if (_synchronizationConfiguration.SynchronizationType == SynchronizationType.BiDirectional
                || _synchronizationConfiguration.SynchronizationType == SynchronizationType.OneWay_SourceToDestination)
            {
                _destinationCalendar.SynchronizeDirtyEvents(synchronizedEvents.DestinationEventList);
                if (_stopRequested)
                {
                    return false;
                }
            }

            if (_synchronizationConfiguration.SynchronizationType == SynchronizationType.BiDirectional
                || _synchronizationConfiguration.SynchronizationType == SynchronizationType.OneWay_DestinationToSource)
            {
                _sourceCalendar.SynchronizeDirtyEvents(synchronizedEvents.SourceEventList);
                if (_stopRequested)
                {
                    return false;
                }
            }

            // Update the latest sync ahead and behind values
            _synchronizationConfiguration.LastSyncBehindDate = syncEventStartDate;
            _synchronizationConfiguration.LastSyncAheadDate = syncEventEndDate;

            return true;
        }


        /// <summary>
        /// Log synchronized event lists.
        /// </summary>
        /// <param name="synchronizedEventLists"></param>
        private void LogSynchronizedEvents(SynchronizedEventLists synchronizedEventLists)
        {
            var infoLog = new LogEventInfo(LogLevel.Info, LOGGER.Name, "Synchronized event lists");
            infoLog.Properties.Add("SourceCalendarId", _synchronizationConfiguration.SourceCalendarConfigurationId);
            infoLog.Properties.Add("DestinationCalendarId", _synchronizationConfiguration.DestinationCalendarConfigurationId);

            infoLog.Properties.Add("SourceEntriesMarkedForCreation",
                                   synchronizedEventLists.SourceEventList.Count(e => e.CreateOnSync));
            infoLog.Properties.Add("SourceEntriesMarkedForDeletion",
                                   synchronizedEventLists.SourceEventList.Count(e => e.DeleteOnSync));
            infoLog.Properties.Add("SourceEntriesMarkedForUndelete",
                                   synchronizedEventLists.SourceEventList.Count(e => e.UnDeleteOnSync));

            infoLog.Properties.Add("DestinationEntriesMarkedForCreation",
                                   synchronizedEventLists.DestinationEventList.Count(e => e.CreateOnSync));
            infoLog.Properties.Add("DestinationEntriesMarkedForDeletion",
                                   synchronizedEventLists.DestinationEventList.Count(e => e.DeleteOnSync));
            infoLog.Properties.Add("DestinationEntriesMarkedForUndelete",
                                   synchronizedEventLists.DestinationEventList.Count(e => e.UnDeleteOnSync));

            LOGGER.Log(infoLog);
        }
    }
}
