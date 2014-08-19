using NUnit.Framework;
using RK.CalendarSync.Core.Configuration.Services;

namespace RK.CalendarSync.Core.UnitTests.Configuration
{
    [TestFixture]
    public class ServiceConfigurationFileRetrieverTests
    {
        [Test]
        public void QuickTest()
        {
            var configurationRetriever =
                new FileBasedServiceConfigurationRetriever("Configuration/TestServiceConfigurations.xml");

            var configurations = configurationRetriever.GetServiceConfigurations();
        }
    }
}
