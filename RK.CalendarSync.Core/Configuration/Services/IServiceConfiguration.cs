namespace RK.CalendarSync.Core.Configuration.Services
{
    /// <summary>
    /// Service configurations needed when accessing services, independent of individual calendar settings.
    /// This is used when accessing things like the Google API, which use common developer keys across all 
    /// API access, but with those keys you can access anyones calendar with consent.
    /// </summary>
    public interface IServiceConfiguration
    {
        /// <summary>
        /// Calendar service type
        /// </summary>
        CalendarType CalendarType { get; }
    }
}
