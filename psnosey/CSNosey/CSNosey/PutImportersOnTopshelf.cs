using System;
using System.Collections.Generic;
using CSNosey.RealTimeImporters;
using PlainElastic.Net;

namespace CSNosey
{
    internal class PutImportersOnTopshelf
    {
        private ElasticConnection _connection;
        private IList<IRealTimeImporter> _importers;

        public void Start()
        {
            _connection = new ElasticConnection("asnav-monitor-01");

            Console.WriteLine(DateTime.Now);

            _importers = new List<IRealTimeImporter>
                {
                    new EventLogRealTimeImporter(),
                    new PerformanceCounterRealTimeImporter()
                };

            foreach (IRealTimeImporter realTimeImporter in _importers)
            {
                realTimeImporter.Begin(_connection);
            }
        }

        public void Stop()
        {
            foreach (IRealTimeImporter realTimeImporter in _importers)
            {
                ((IDisposable) realTimeImporter).Dispose();
            }
        }
    }
}