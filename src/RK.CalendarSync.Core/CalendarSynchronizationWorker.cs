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
        /// If we fail to sync, wait 2.5 minutes before retrying.
        /// </summary>
        private const int DEFAULT_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL = 150000;

        /// <summary>
        /// At a maximum, wait 6 hours.
        /// </summary>
        private const int MAX_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL = 216000000;

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
            var numberOfSequentialFailures = 0;

            while (!_stopRequested)
            {
                _syncWaitEvent.WaitOne(millisecondsBeforeNextSync);

                if (_stopRequested)
                {
                    break;
                }

                // Log that we're synchronizing
                LOGGER.Info("Attempting to synchronize calendars {0} and {1}",
                                          _synchronizationConfiguration.SourceCalendarConfigurationId,
                                          _synchronizationConfiguration.DestinationCalendarConfigurationId);

                // If the sync succeeds, determine how long to wait and write the success time back to disk.
                var attemptedSyncTime = DateTimeOffset.Now;
                try
                {
                    SynchronizeAndUpdate();
                
                    _synchronizationConfiguration.LastSynchronization = attemptedSyncTime;
                    
                    // Indicate to whoever cares that we should re-save the configuration settings.
                    _saveSynchronizationConfigurationsEvent.Set();

                    nextSyncTime =
                        _synchronizationConfiguration.LastSynchronization.GetValueOrDefault(DateTimeOffset.MinValue)
                                                     .AddMinutes(_synchronizationConfiguration.MinutesBetweenSynchronization);
                    millisecondsBeforeNextSync = (int)Math.Max(nextSyncTime.Subtract(DateTimeOffset.Now).TotalMilliseconds, 0);

                    // We had a success!
                    numberOfSequentialFailures = 0;

                }
                catch (Exception ex)
                {
                    // If we failed, then simply wait a deteremined amount of time before retrying
                    //
                    // We probably want to have some sort of "retry strategy" object here instead, but for now
                    // just do an exponential backoff (failures are most likely due to a service resource being down.)
                    millisecondsBeforeNextSync = DEFAULT_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL*
                                                 (int) Math.Pow(2, numberOfSequentialFailures);
                    millisecondsBeforeNextSync = millisecondsBeforeNextSync > MAX_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL
                                                     ? MAX_MILLISECOND_WAIT_TIME_ON_SYNC_FAIL
                                                     : millisecondsBeforeNextSync;

                    numberOfSequentialFailures++;

                    LOGGER.Error(
                        string.Format(
                            "Failed to synchronize events between {0} and {1}, waiting {2} milliseconds before retry.",
                            _synchronizationConfiguration.SourceCalendarConfigurationId,
                            _synchronizationConfiguration.DestinationCalendarConfigurationId, millisecondsBeforeNextSync),
                        ex);

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
        private void SynchronizeAndUpdate()
        {
            var syncEventStartDate = DateTimeOffset.Now.AddDays(0 - _synchronizationConfiguration.DaysInPastToSync);
            var syncEventEndDate = DateTimeOffset.Now.AddDays(_synchronizationConfiguration.DaysInFutureToSync);

            var sourceCalendarEvents = _sourceCalendar.GetCalendarEvents(syncEventStartDate, syncEventEndDate);
            if (_stopRequested)
            {
                return;
            }
            var destinationCalendarEvents = _destinationCalendar.GetCalendarEvents(syncEventStartDate, syncEventEndDate);
            if (_stopRequested)
            {
                return;
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
                return;
            }

            if (_synchronizationConfiguration.SynchronizationType == SynchronizationType.BiDirectional
                || _synchronizationConfiguration.SynchronizationType == SynchronizationType.OneWay_SourceToDestination)
            {
                // See if sync is successful
                _destinationCalendar.SynchronizeDirtyEvents(synchronizedEvents.DestinationEventList);
            }

            // Before we sync the next set see if a stop is requested.
            if (_stopRequested)
            {
                return;
            }

            if (_synchronizationConfiguration.SynchronizationType == SynchronizationType.BiDirectional
                || _synchronizationConfiguration.SynchronizationType == SynchronizationType.OneWay_DestinationToSource)
            {
                // See if sync is successful
                _sourceCalendar.SynchronizeDirtyEvents(synchronizedEvents.SourceEventList);
            }

            // Update the latest sync ahead and behind values
            _synchronizationConfiguration.LastSyncBehindDate = syncEventStartDate;
            _synchronizationConfiguration.LastSyncAheadDate = syncEventEndDate;
        }


        /// <summary>
        /// Log synchronized event lists.
        /// </summary>
        /// <param name="synchronizedEventLists"></param>
        private void LogSynchronizedEvents(SynchronizedEventLists synchronizedEventLists)
        {
            LOGGER.Info("Source calendar ({0}): {1} total events, {2} marked for creation, {3} marked for deletion, {4} marked for undelete.",
                _synchronizationConfiguration.SourceCalendarConfigurationId,
                synchronizedEventLists.SourceEventList.Count(),
                synchronizedEventLists.SourceEventList.Count(e => e.CreateOnSync),
                synchronizedEventLists.SourceEventList.Count(e => e.DeleteOnSync),
                synchronizedEventLists.SourceEventList.Count(e => e.UnDeleteOnSync));

            LOGGER.Info("Destination calendar ({0}): {1} total events, {2} marked for creation, {3} marked for deletion, {4} marked for undelete.",
                _synchronizationConfiguration.DestinationCalendarConfigurationId,
                synchronizedEventLists.DestinationEventList.Count(),
                synchronizedEventLists.DestinationEventList.Count(e => e.CreateOnSync),
                synchronizedEventLists.DestinationEventList.Count(e => e.DeleteOnSync),
                synchronizedEventLists.DestinationEventList.Count(e => e.UnDeleteOnSync));
        }
    }
}
