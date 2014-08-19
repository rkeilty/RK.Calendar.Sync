using System;
using Microsoft.Exchange.WebServices.Data;

namespace RK.CalendarSync.Core.Configuration.Calendars
{
    /// <summary>
    /// Contains configuration data for connecting to an Exchange server calendar
    /// </summary>
    public class ExchangeCalendarConfiguration : ICalendarConfiguration
    {
        /// <summary>
        /// Full email/username for connecting (jon.doe@microsoft.com)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Optional if not using domain credentials, specify the password for the account.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Optional, if you specify this the service will attempt to use the currently logged in users
        /// domain credentials so you don't have to supply a password.
        /// </summary>
        public bool UseDomainCredentials { get; set; }

        /// <summary>
        /// The endpoint the Exchange server is hosted on
        /// </summary>
        public string Endpoint { get; set; }

        /// <summary>
        /// The version of the Exchange server to connect to
        /// </summary>
        public ExchangeVersion ExchangeVersion { get; set; }

        /// <summary>
        /// Unique configuration ID, used when determining what configurations to sync
        /// </summary>
        public Guid CalendarConfigurationId { get; set; }

        /// <summary>
        /// Calendar type, as required by ICalendarConfiguration
        /// </summary>
        public CalendarType CalendarType { get { return CalendarType.Exchange; } }

        public ExchangeCalendarConfiguration()
        { }

        public ExchangeCalendarConfiguration(string userName,
                                                    string password,
                                                    bool useDomainCredentials,
                                                    string endpoint,
                                                    ExchangeVersion exchangeVersion)
        {
            UserName = userName;
            Password = password;
            UseDomainCredentials = useDomainCredentials;
            Endpoint = endpoint;
            ExchangeVersion = exchangeVersion;
        }
    }
}
