using System;
using System.Collections.Generic;
using NUnit.Framework;
using RK.CalendarSync.Core;
using RK.CalendarSync.Core.Configuration.Synchronization;

namespace RK.CalendarSync.UnitTests.Configuration
{
    [TestFixture]
    public class SynchronizationConfigurationReaderWriterTests
    {
        [Test]
        public void QuickReadTest()
        {
            var synchronizationConfigurationReaderWriter =
                new FileBasedSynchronizationConfigurationReaderWriter("Configuration/TestSyncConfigurations.xml");

            synchronizationConfigurationReaderWriter.GetSynchronizationConfigurations();
        }

        [Test]
        public void QuickWriteTest()
        {
            var synchronizationConfigurationReaderWriter =
                new FileBasedSynchronizationConfigurationReaderWriter("Configuration/TestSyncConfigurations.xml");

            var config = new SynchronizationConfiguration(Guid.NewGuid(), Guid.NewGuid(),
                                                          SynchronizationType.BiDirectional,
                                                          2, 10, 60, null, null, null);

            synchronizationConfigurationReaderWriter.SaveSynchronizationConfigurations(new List<ISynchronizationConfiguration>(){config});
        }

        [Test]
        public void QuickWriteThenReadTest()
        {
            var synchronizationConfigurationReaderWriter =
                new FileBasedSynchronizationConfigurationReaderWriter("Configuration/TestSyncConfigurations.xml");

            var config = new SynchronizationConfiguration(Guid.NewGuid(), Guid.NewGuid(),
                                                          SynchronizationType.BiDirectional,
                                                          2, 10, 60, DateTimeOffset.Now, null, null);

            synchronizationConfigurationReaderWriter.SaveSynchronizationConfigurations(new List<ISynchronizationConfiguration>() { config });

            var updatedSyncEvents = synchronizationConfigurationReaderWriter.GetSynchronizationConfigurations();
        }
    }
}
