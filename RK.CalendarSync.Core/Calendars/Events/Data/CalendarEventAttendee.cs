using System;
using System.Collections.Generic;

namespace RK.CalendarSync.Core.Calendars.Events.Data
{
    internal class CalendarEventAttendee : IEquatable<CalendarEventAttendee>, IEqualityComparer<CalendarEventAttendee>
    {
        public CalendarEventAttendee(string displayName, string email)
        {
            _displayName = displayName;
            _email = email;
        }

        /// <summary>
        /// CalendarEventAttendee display name.
        /// </summary>
        public string DisplayName { get { return _displayName; } }
        private readonly string _displayName;

        /// <summary>
        /// CalendarEventAttendee email
        /// </summary>
        public string Email { get { return _email; } }
        private readonly string _email;

        /// <summary>
        /// Equality overrides
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            var calendarEventAttendee = obj as CalendarEventAttendee;
            if (calendarEventAttendee != null)
            {
                return Equals(calendarEventAttendee);
            }

            return false;
        }

        /// <summary>
        /// Equality overrides
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(CalendarEventAttendee other)
        {
            return Equals(this, other);
        }

        /// <summary>
        /// Equality overrides
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(CalendarEventAttendee x, CalendarEventAttendee y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }

            if (ReferenceEquals(y, null))
            {
                return false;
            }

            return ((x.DisplayName ?? String.Empty).Equals(y.DisplayName ?? string.Empty))
                   && ((x.Email ?? String.Empty).Equals(y.Email ?? string.Empty));
        }


        /// <summary>
        /// Equals override
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static bool operator ==(CalendarEventAttendee x, CalendarEventAttendee y)
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
        public static bool operator !=(CalendarEventAttendee x, CalendarEventAttendee y)
        {
            // Null object checks
            if (ReferenceEquals(x, null))
            {
                return !ReferenceEquals(y, null);
            }

            return !x.Equals(y);
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
        public int GetHashCode(CalendarEventAttendee obj)
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = (int)2166136261;
                // Suitable nullity checks etc, of course
                hash = hash * 16777619 ^ (obj.DisplayName ?? string.Empty).GetHashCode();
                hash = hash * 16777619 ^ (obj.Email ?? string.Empty).GetHashCode();
                return hash;
            }
        }
    }
}