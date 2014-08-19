namespace RK.CalendarSync.Core.Configuration.Services
{
    /// <summary>
    /// Specific credentials needed when accessing Google APIs, independent of individual calendar settings.
    /// </summary>
    public class GoogleServiceConfiguration : IServiceConfiguration
    {
        /// <summary>
        /// The client ID used when accessing the Google API
        /// </summary>
        public string ApiClientId { get; set; }

        /// <summary>
        /// The client secret used when accessing the Google APIs
        /// </summary>
        public string ApiClientSecret { get; set; }

        /// <summary>
        /// The calendar type the service configuration applies to.
        /// </summary>
        public CalendarType CalendarType { get {return CalendarType.Google;} }

        public GoogleServiceConfiguration()
        { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiClientId"></param>
        /// <param name="apiClientSecret"></param>
        public GoogleServiceConfiguration(string apiClientId, string apiClientSecret)
        {
            ApiClientId = apiClientId;
            ApiClientSecret = apiClientSecret;
        }
    }
}
