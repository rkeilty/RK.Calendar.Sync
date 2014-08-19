using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NLog;
using RK.CalendarSync.Core.Configuration.Calendars;
using RK.CalendarSync.Core.Configuration.Services;
using RK.CalendarSync.Core.Configuration.Synchronization;

namespace RK.CalendarSync.Core
{
    public class CalendarSyncService : ICalendarSyncService
    {
        /// <summary>
        /// Logger for the class
        /// </summary>
        private static readonly Logger LOGGER = LogManager.GetCurrentClassLogger();
        private const int DEFAULT_MILLISECOND_WAIT_TIME_FOR_THREAD_STOP_ON_SHUTDOWN = 10000;

        private readonly ICalendarConfigurationRetriever _calendarConfigurationRetriever;
        private readonly ISynchronizationConfigurationReaderWriter _synchronizationConfigurationReaderWriter;
        private readonly IServiceConfigurationRetriever _serviceConfigurationRetriever;
        private readonly ICalendarSynchronizationWorkerFactory _calendarSynchronizationWorkerFactory;
        private readonly AutoResetEvent _saveSynchronizationConfigurationsEvent = new AutoResetEvent(false);
        private IList<ICalendarSynchronizationWorker> _syncWorkers = new List<ICalendarSynchronizationWorker>();
        private IList<Thread> _syncWorkerThreads = new List<Thread>();
        private Thread _syncConfigSaveThread;
        private bool _stopRequested = false;

        /// <summary>
        /// Default constructor
        /// </summary>
        public CalendarSyncService()
        {
            _calendarConfigurationRetriever = new FileBasedCalendarConfigurationRetriever();
            _synchronizationConfigurationReaderWriter = new FileBasedSynchronizationConfigurationReaderWriter();
            _serviceConfigurationRetriever = new FileBasedServiceConfigurationRetriever();
            _calendarSynchronizationWorkerFactory = new CalendarSynchronizationWorkerFactory(_saveSynchronizationConfigurationsEvent);
        }

        /// <summary>
        /// Useful if unit tests or others want to pass in their own configuration retrievers.  Keep as internal for now.
        /// </summary>
        /// <param name="calendarConfigurationRetriever"></param>
        /// <param name="synchronizationConfigurationReaderWriter"></param>
        /// <param name="serviceConfigurationRetriever"></param>
        /// <param name="calendarSynchronizationWorkerFactory"></param>
        internal CalendarSyncService(
            ICalendarConfigurationRetriever calendarConfigurationRetriever,
            ISynchronizationConfigurationReaderWriter synchronizationConfigurationReaderWriter,
            IServiceConfigurationRetriever serviceConfigurationRetriever,
            ICalendarSynchronizationWorkerFactory calendarSynchronizationWorkerFactory)
        {
            _calendarConfigurationRetriever = calendarConfigurationRetriever;
            _synchronizationConfigurationReaderWriter = synchronizationConfigurationReaderWriter;
            _serviceConfigurationRetriever = serviceConfigurationRetriever;
            _calendarSynchronizationWorkerFactory = calendarSynchronizationWorkerFactory;
        }

        /// <summary>
        /// Starts the service and underlying synchronization
        /// </summary>
        public void Start()
        {
            // Synchronization service stopped successfully.
            LOGGER.Info("Starting all background synchronization services.");

            // Get the calendar configurations
            var calendarConfigs =_calendarConfigurationRetriever.GetCalendarConfigurations();

            // Grab the sync configurations
            var syncConfigs = _synchronizationConfigurationReaderWriter.GetSynchronizationConfigurations();

            // Retrieve any service configurations (Google API credentials, etc.)
            var serviceConfigs = _serviceConfigurationRetriever.GetServiceConfigurations();

            // Given sync configurations, service level credentials, create the calendars and store them in a list.
            foreach (var syncConfig in syncConfigs)
            {
                var syncWorker = _calendarSynchronizationWorkerFactory.GetSynchronizationWorker(syncConfig,
                                                                                                calendarConfigs,
                                                                                                serviceConfigs);

                _syncWorkers.Add(syncWorker);
            }

            // Now that we've properly initialized all the worker threads, start 'em up.
            foreach (var syncWorker in _syncWorkers)
            {
                var syncThread = new Thread(syncWorker.Start);
                syncThread.Start();
                _syncWorkerThreads.Add(syncThread);
            }

            // Start the sync-saver thread
            _syncConfigSaveThread = new Thread(SaveSynchronizationWorker);
            _syncConfigSaveThread.Start();

            // Synchronization service started successfully.
            LOGGER.Info("Started all background synchronization services.");
        }


        /// <summary>
        /// Stops the service and underlying synchronization
        /// </summary>
        public void Stop()
        {
            // Synchronization service stopped successfully.
            LOGGER.Info("Stopping all background synchronization services.");

            _stopRequested = true;

            // Iterate over each of our workers and tell them to stop
            foreach (var syncWorker in _syncWorkers)
            {
                syncWorker.Stop();
            }

            // Wait one second, then murder the thread
            foreach (var syncThread in _syncWorkerThreads)
            {
                syncThread.Join(DEFAULT_MILLISECOND_WAIT_TIME_FOR_THREAD_STOP_ON_SHUTDOWN);
                if (syncThread.IsAlive)
                {
                    syncThread.Abort();
                }
            }

            // Trigger one final save event on stop requested.
            _saveSynchronizationConfigurationsEvent.Set();

            // Make sure the final save actually finished, kill it otherwise.
            _syncConfigSaveThread.Join(DEFAULT_MILLISECOND_WAIT_TIME_FOR_THREAD_STOP_ON_SHUTDOWN);
            if (_syncConfigSaveThread.IsAlive)
            {
                _syncConfigSaveThread.Abort();
            }

            // Synchronization service stopped successfully.
            LOGGER.Info("Stopped all background synchronization services.");
        }

        /// <summary>
        /// Helper method which will trigger synchronization configuration saves.  Probably doesn't belong here, but 
        /// will suffice for this early version.
        /// </summary>
        private void SaveSynchronizationWorker()
        {
            while (!_stopRequested)
            {
                _saveSynchronizationConfigurationsEvent.WaitOne(Timeout.Infinite);

                // Synchronization service stopped successfully.
                LOGGER.Info("Synchronization save triggered.");

                SaveSynchronizationConfigurations();
            }
        }


        /// <summary>
        /// Given a specific configuration, save it to our backing store.
        /// </summary>
        public void SaveSynchronizationConfigurations()
        {
            // Retrieve all of the current synchronization configurations.
            var existingSyncConfigs = _syncWorkers.Select(s => s.SynchronizationConfiguration).ToList();

            // Save the existing configurations back.
            _synchronizationConfigurationReaderWriter.SaveSynchronizationConfigurations(existingSyncConfigs);
        }


        /// <summary>
        /// Helper method to return calendars from service
        /// </summary>
        /// <returns></returns>
        public IDictionary<Guid, ICalendarConfiguration> GetCalendarConfigurations()
        {
            return _calendarConfigurationRetriever.GetCalendarConfigurations();
        }
    }
}
