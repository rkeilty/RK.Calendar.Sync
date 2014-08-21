using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace RK.CalendarSync.Core.Configuration.Synchronization
{
    public class SynchronizationConfiguration : ISynchronizationConfiguration, IXmlSerializable
    {
        /// <summary>
        /// Source calendar configuration ID to sync.
        /// </summary>
        public Guid SourceCalendarConfigurationId { get; set; }

        /// <summary>
        /// Destination calendar configuration ID to sync.
        /// </summary>
        public Guid DestinationCalendarConfigurationId { get; set; }

        /// <summary>
        /// What type of synchronization do we perform between the calendars.
        /// </summary>
        public SynchronizationType SynchronizationType { get; set; }

        /// <summary>
        /// Number of days in the past syncing occurs
        /// </summary>
        public int DaysInPastToSync { get; set; }

        /// <summary>
        /// Number of days in the future syncing occurs
        /// </summary>
        public int DaysInFutureToSync { get; set; }

        /// <summary>
        /// Number of minutes to wait between synchronizations.
        /// </summary>
        public int MinutesBetweenSynchronization { get; set; }

        /// <summary>
        /// Last successful synchronization
        /// </summary>
        public DateTimeOffset? LastSynchronization { get; set; }

        /// <summary>
        /// The date in the past the last synchronization went back to.
        /// </summary>
        public DateTimeOffset? LastSyncBehindDate { get; set; }

        /// <summary>
        /// The date in the future the last synchronization went ahead to.
        /// </summary>
        public DateTimeOffset? LastSyncAheadDate { get; set; }

        public SynchronizationConfiguration()
        { }

        public SynchronizationConfiguration(Guid sourceCalendarConfigurationId,
                                            Guid destinationCalendarConfigurationId,
                                            SynchronizationType synchronizationType,
                                            int daysInPastToSync,
                                            int daysInFutureToSync,
                                            int minutesBetweenSynchronization,
                                            DateTimeOffset? lastSynchronization,
                                            DateTimeOffset? lastSyncBehindDate,
                                            DateTimeOffset? lastSyncAheadDate)
        {
            SourceCalendarConfigurationId = sourceCalendarConfigurationId;
            DestinationCalendarConfigurationId = destinationCalendarConfigurationId;
            SynchronizationType = synchronizationType;
            DaysInPastToSync = daysInPastToSync;
            DaysInFutureToSync = daysInFutureToSync;
            MinutesBetweenSynchronization = minutesBetweenSynchronization;
            LastSynchronization = lastSynchronization;
            LastSyncBehindDate = lastSyncBehindDate;
            LastSyncAheadDate = lastSyncAheadDate;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Overwrite how we deserialize object DateTimeOffset
        /// </summary>
        /// <param name="reader"></param>
        public void ReadXml(XmlReader reader)
        {
            var rawStringElementValues = new Dictionary<string, string>();

            reader.MoveToContent();
            reader.ReadStartElement();
            
            // We know we should have 7 elements, so just iterate over each sub-element.
            while (rawStringElementValues.Count < 9)
            {
                var element = reader.Name;
                var value = reader.ReadElementString();
                rawStringElementValues.Add(element, value);
            }

            SourceCalendarConfigurationId = Guid.Parse(rawStringElementValues["SourceCalendarConfigurationId"]);
            DestinationCalendarConfigurationId = Guid.Parse(rawStringElementValues["DestinationCalendarConfigurationId"]);
            SynchronizationType = (SynchronizationType)Enum.Parse(typeof(SynchronizationType), rawStringElementValues["SynchronizationType"]);
            DaysInPastToSync = int.Parse(rawStringElementValues["DaysInPastToSync"]);
            DaysInFutureToSync = int.Parse(rawStringElementValues["DaysInFutureToSync"]);
            MinutesBetweenSynchronization = int.Parse(rawStringElementValues["MinutesBetweenSynchronization"]);

            var lastSynchronizationString = rawStringElementValues["LastSynchronization"];
            LastSynchronization = string.IsNullOrEmpty(lastSynchronizationString)
                                                 ? DateTimeOffset.MinValue
                                                 : DateTimeOffset.Parse(lastSynchronizationString);

            var lastSyncBehindDateString = rawStringElementValues["LastSyncBehindDate"];
            LastSyncBehindDate = string.IsNullOrEmpty(lastSyncBehindDateString)
                                                 ? DateTimeOffset.MinValue
                                                 : DateTimeOffset.Parse(lastSyncBehindDateString);

            var lastSyncAheadDateString = rawStringElementValues["LastSyncAheadDate"];
            LastSyncAheadDate = string.IsNullOrEmpty(lastSyncAheadDateString)
                                                 ? DateTimeOffset.MinValue
                                                 : DateTimeOffset.Parse(lastSyncAheadDateString);

            reader.ReadEndElement();
        }

        /// <summary>
        /// Overwrite how we serialize since DateTimeOffset can't be serialized on it's own.
        /// </summary>
        /// <param name="writer"></param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("SourceCalendarConfigurationId", SourceCalendarConfigurationId.ToString("D"));
            writer.WriteElementString("DestinationCalendarConfigurationId", DestinationCalendarConfigurationId.ToString("D"));
            writer.WriteElementString("SynchronizationType", SynchronizationType.ToString("G"));
            writer.WriteElementString("DaysInPastToSync", DaysInPastToSync.ToString());
            writer.WriteElementString("DaysInFutureToSync", DaysInFutureToSync.ToString());
            writer.WriteElementString("MinutesBetweenSynchronization", MinutesBetweenSynchronization.ToString());

            // ISO 8601 format
            writer.WriteElementString("LastSynchronization", LastSynchronization.GetValueOrDefault(DateTimeOffset.MinValue).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
            writer.WriteElementString("LastSyncBehindDate", LastSyncBehindDate.GetValueOrDefault(DateTimeOffset.MinValue).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
            writer.WriteElementString("LastSyncAheadDate", LastSyncAheadDate.GetValueOrDefault(DateTimeOffset.MinValue).ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz"));
        }
    }
}
