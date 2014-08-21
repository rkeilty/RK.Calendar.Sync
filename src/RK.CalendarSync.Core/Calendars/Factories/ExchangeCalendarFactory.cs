using System;
using Microsoft.Exchange.WebServices.Data;
using RK.CalendarSync.Core.Configuration.Calendars;

namespace RK.CalendarSync.Core.Calendars.Factories
{
    internal class ExchangeCalendarFactory : ICalendarFactory
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _exchangeServerUrl;
        private readonly bool _useDomainCredentials;
        private readonly ExchangeVersion _exchangeVersion;
        private ExchangeService _exchangeService;

        private ExchangeCalendarFactory(string exchangeServerUrl, ExchangeVersion exchangeVersion)
        {
            _exchangeServerUrl = exchangeServerUrl;
            _exchangeVersion = exchangeVersion;
            _useDomainCredentials = true;
            InitializeExchangeService();
        }

        private ExchangeCalendarFactory(string username, string password, string exchangeServerUrl, ExchangeVersion exchangeVersion)
        {
            _username = username;
            _password = password;
            _exchangeServerUrl = exchangeServerUrl;
            _exchangeVersion = exchangeVersion;
            _useDomainCredentials = false;
            InitializeExchangeService();
        }

        /// <summary>
        /// Create a factory class based on the calendar configuration
        /// </summary>
        /// <param name="calendarConfiguration"></param>
        /// <returns></returns>
        public static ExchangeCalendarFactory CreateFromCalendarConfiguration(ExchangeCalendarConfiguration calendarConfiguration)
        {
            if (calendarConfiguration.UseDomainCredentials)
            {
                return new ExchangeCalendarFactory(calendarConfiguration.Endpoint, calendarConfiguration.ExchangeVersion);
            }
            else
            {
                return new ExchangeCalendarFactory(calendarConfiguration.UserName, calendarConfiguration.Password,
                                                   calendarConfiguration.Endpoint, calendarConfiguration.ExchangeVersion);
            }
        }

        /// <summary>
        /// Get the users exchange calendar.
        /// </summary>
        /// <returns></returns>
        public ICalendar GetCalendar()
        {
            return new ExchangeCalendar(_exchangeService);
        }

        /// <summary>
        /// Helper method to initialize the exchange service.
        /// </summary>
        private void InitializeExchangeService()
        {
            // Instantiate and talk to the Exchange server
            _exchangeService = new ExchangeService(_exchangeVersion);
            _exchangeService.Url = new Uri(_exchangeServerUrl);

            // Setup credentials (domain vs username)
            if (_useDomainCredentials)
            {
                _exchangeService.UseDefaultCredentials = true;
            }
            else
            {
                _exchangeService.Credentials = new WebCredentials(_username, _password);
            }
        }

        /// <summary>
        /// Previously used for auto-discovery, shoudld move to appropriate helper class eventually.
        /// </summary>
        /// <param name="redirectionUrl"></param>
        /// <returns></returns>
        private static bool RedirectionUrlValidationCallback(string redirectionUrl)
        {
            // The default for the validation callback is to reject the URL.
            bool result = false;

            Uri redirectionUri = new Uri(redirectionUrl);

            // Validate the contents of the redirection URL. In this simple validation
            // callback, the redirection URL is considered valid if it is using HTTPS
            // to encrypt the authentication credentials. 
            if (redirectionUri.Scheme == "https")
            {
                result = true;
            }
            return result;
        }
    }
}
