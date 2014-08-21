using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using RK.CalendarSync.Core.Calendars.Services.Google;
using RK.CalendarSync.Core.Configuration.Calendars;
using RK.CalendarSync.Core.Configuration.Services;

namespace RK.CalendarSync.Core.Calendars.Factories
{
    internal class GoogleCalendarFactory : ICalendarFactory
    {
        private readonly GoogleCalendarConfiguration _calendarConfiguration;
        private readonly GoogleServiceConfiguration _serviceConfiguration;

        private IGoogleCalendarService _calendarService;

        public GoogleCalendarFactory(GoogleCalendarConfiguration calendarConfiguration, GoogleServiceConfiguration serviceConfiguration)
        {
            _calendarConfiguration = calendarConfiguration;
            _serviceConfiguration = serviceConfiguration;
            InitializeCalendarService();
        }

        public ICalendar GetCalendar()
        {
            return new GoogleCalendar(_calendarService, _calendarConfiguration.CalendarId);
        }

        /// <summary>
        /// Initialize and authorize the user
        /// </summary>
        private void InitializeCalendarService()
        {
            // Make the credentials 
            var credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = _serviceConfiguration.ApiClientId,
                    ClientSecret = _serviceConfiguration.ApiClientSecret,
                },
                new[] { CalendarService.Scope.Calendar },
                _calendarConfiguration.UserName,
                CancellationToken.None).Result;


            // Create the service.
            var calendarService = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApplicationName = "RK Calendar Sync",
            });

            // Drop it in our wrapper.
            _calendarService = new GoogleCalendarService(calendarService);
        }
    }
}
