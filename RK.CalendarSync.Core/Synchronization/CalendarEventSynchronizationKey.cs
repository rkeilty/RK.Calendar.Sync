using System;
using System.Collections.Generic;

namespace RK.CalendarSync.Core.Synchronization
{
    internal class CalendarEventSynchronizationKey : 
        IEquatable<CalendarEventSynchronizationKey>, 
        IEqualityComparer<CalendarEventSynchronizationKey>, 
        IComparable<CalendarEventSynchronizationKey>,
        IComparer<CalendarEventSynchronizationKey>
    {
        /// <summary>
        /// Synchronization key for nonrecurring events
        /// </summary>
        /// <param name="uid"></param>
        public CalendarEventSynchronizationKey(string uid)
        {
            UID = uid;
        }

        /// <summary>
        /// Synchronization key for events that have recurrenceIDs
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="recurrenceId"></param>
        public CalendarEventSynchronizationKey(string uid, DateTimeOffset? recurrenceId)
        {
            UID = uid;
            RecurrenceId = recurrenceId;
        }

        /// <summary>
        /// iCal UID
        /// </summary>
        public string UID { get; private set; }

        /// <summary>
        /// Recurrence ID in iCal format
        /// </summary>
        public DateTimeOffset? RecurrenceId { get; private set; }
        

        /// <summary>
        /// Determines key object equality
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        bool IEqualityComparer<CalendarEventSynchronizationKey>.Equals(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            return Equals(x, y);
        }

        /// <summary>
        /// Determines key object equality
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var key = obj as CalendarEventSynchronizationKey;
            if (key != null)
            {
                return Equals(this, key);
            }
            
            return false;
        }

        /// <summary>
        /// Determines key object equality
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(CalendarEventSynchronizationKey other)
        {
            return Equals(this, other);
        }

        /// <summary>
        /// Determines key object equality
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool Equals(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            // Null object checks
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return ((x.UID == null && y.UID == null)
                       || (x.UID != null && y.UID != null && x.UID.Equals(y.UID)))
                   && ((x.RecurrenceId == null && y.RecurrenceId == null)
                       || (x.RecurrenceId != null && y.RecurrenceId != null && x.RecurrenceId.Equals(y.RecurrenceId)));
        }

        /// <summary>
        /// FNV hashcode implementation
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return GetHashCode(this);
        }

        /// <summary>
        /// FNV hashcode implementation
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int GetHashCode(CalendarEventSynchronizationKey obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = (int)2166136261;
                // Suitable nullity checks etc, of course
                hash = hash * 16777619 ^ (obj.UID ?? string.Empty).GetHashCode();
                hash = hash * 16777619 ^ ((obj.RecurrenceId == null) ? 0 : obj.RecurrenceId.GetHashCode());
                return hash;
            }
        }

        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            // Null object checks
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }

            return x.Equals(y);
        }

        /// <summary>
        /// Not equals override
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator !=(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            // Null object checks
            if (ReferenceEquals(x, null))
            {
                return !ReferenceEquals(y, null);
            }

            return !x.Equals(y);
        }

        /// <summary>
        /// Less than
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator <(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            return Compare(x, y) < 0;
        }

        /// <summary>
        /// Less than or equal
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator <=(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            return Compare(x, y) >= 0;
        }

        /// <summary>
        /// Greater than
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator >(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            return Compare(x, y) > 0;
        }

        /// <summary>
        /// Greater than or equal
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator >=(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            return Compare(x, y) <= 0;
        }

        /// <summary>
        /// Implementing standard comparer objects
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CalendarEventSynchronizationKey other)
        {
            // Null ref checks
            if (ReferenceEquals(other, null))
            {
                return -1;
            }

            return Compare(this, other);
        }

        
        /// <summary>
        /// Implementing standard comparer objects
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int IComparer<CalendarEventSynchronizationKey>.Compare(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            return Compare(x, y);
        }


        /// <summary>
        /// Implementing standard comparer objects
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static int Compare(CalendarEventSynchronizationKey x, CalendarEventSynchronizationKey y)
        {
            // Null ref checks
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null) ? 0 : -1;
            }

            if (ReferenceEquals(y, null))
            {
                return 1;
            }

            var xUid = x.UID ?? string.Empty;
            var yUid = y.UID ?? string.Empty;

            var uidCompareValue = xUid.CompareTo(yUid);

            // If the UIDs are not equal, return the value
            if (uidCompareValue != 0)
            {
                return uidCompareValue;
            }

            // Otherwise compare the recurrence ids
            // Null ref checks
            if (ReferenceEquals(x.RecurrenceId, null))
            {
                return ReferenceEquals(y.RecurrenceId, null) ? 0 : -1;
            }

            if (ReferenceEquals(y.RecurrenceId, null))
            {
                return 1;
            }

            return x.RecurrenceId.Value.CompareTo(y.RecurrenceId.Value);
        }
    }
}
