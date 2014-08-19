using System;

namespace RK.CalendarSync.Core.Configuration.Synchronization
{
    public interface ISynchronizationConfiguration
    {
        /// <summary>
        /// Source calendar configuration ID to sync.
        /// </summary>
        Guid SourceCalendarConfigurationId { get; set; }

        /// <summary>
        /// Destination calendar configuration ID to sync.
        /// </summary>
        Guid DestinationCalendarConfigurationId { get; set; }

        /// <summary>
        /// What type of synchronization do we perform between the calendars.
        /// </summary>
        SynchronizationType SynchronizationType { get; set; }

        /// <summary>
        /// Number of days in the past syncing occurs
        /// </summary>
        int DaysInPastToSync { get; set; }

        /// <summary>
        /// Number of days in the future syncing occurs
        /// </summary>
        int DaysInFutureToSync { get; set; }

        /// <summary>
        /// Number of minutes to wait between synchronizations.
        /// </summary>
        int MinutesBetweenSynchronization { get; set; }

        /// <summary>
        /// Last successful synchronization
        /// </summary>
        DateTimeOffset? LastSynchronization { get; set; }

        /// <summary>
        /// The date in the past the last synchronization went back to.
        /// </summary>
        DateTimeOffset? LastSyncBehindDate { get; set; }

        /// <summary>
        /// The date in the future the last synchronization went ahead to.
        /// </summary>
        DateTimeOffset? LastSyncAheadDate { get; set; }
    }
}
