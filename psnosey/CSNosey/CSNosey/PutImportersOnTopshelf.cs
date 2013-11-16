﻿using System;
using System.Collections.Generic;
using CSNosey.RealTimeImporters;
using MongoDB.Driver;
using PlainElastic.Net;
using Topshelf;

namespace CSNosey
{
    internal class PutImportersOnTopshelf
    {
        private IDbLogger _connection;
        private IList<IRealTimeImporter> _importers;
        private AutomaticUpdater _automaticUpdater;

        public PutImportersOnTopshelf()
        {
            _automaticUpdater = new AutomaticUpdater();
        }

        public bool Start(HostControl control)
        {
            _automaticUpdater.Start(control);

//            _connection = new ElasticSearchDbLogger();
            _connection = new MongoDbLogger();


            Console.WriteLine(DateTime.Now);

            _importers = new List<IRealTimeImporter>
                {
                    new EventLogRealTimeImporter(),
                    new PerformanceCounterRealTimeImporter(),
                    new HeartBeatRealTimeImporter()
                };

            foreach (IRealTimeImporter realTimeImporter in _importers)
            {
                realTimeImporter.Begin(_connection);
            }

            return true;
        }

        public void Stop()
        {
            foreach (IRealTimeImporter realTimeImporter in _importers)
            {
                ((IDisposable) realTimeImporter).Dispose();
            }

            _automaticUpdater.Stop();
        }
    }
}