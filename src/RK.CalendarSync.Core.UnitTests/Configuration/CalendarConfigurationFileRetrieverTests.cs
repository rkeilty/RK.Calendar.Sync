using NUnit.Framework;
using RK.CalendarSync.Core.Configuration.Calendars;

namespace RK.CalendarSync.Core.UnitTests.Configuration
{
    [TestFixture]
    public class CalendarConfigurationFileRetrieverTests
    {
        [Test]
        public void QuickTest()
        {
            var configurationRetriever =
                new FileBasedCalendarConfigurationRetriever("Configuration/TestCalendarConfiguration.xml");

            var configurations = configurationRetriever.GetCalendarConfigurations();
        }
    }
}
