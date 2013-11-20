using System;
using System.Collections.Generic;
using CSNosey.RealTimeImporters;
using Topshelf;

namespace CSNosey
{
    internal class PutImportersOnTopshelf
    {
        private readonly ITime _time;
        private IDbLogger _connection;
        private IList<IRealTimeImporter> _importers;
        private readonly AutomaticUpdater _automaticUpdater;

        public PutImportersOnTopshelf(ITime time)
        {
            _time = time;
            _automaticUpdater = new AutomaticUpdater(time);
        }

        public bool Start(HostControl control)
        {
            _connection = new ElasticSearchDbLogger(_time);

            _automaticUpdater.Start(control, _connection);

            _importers = new List<IRealTimeImporter>
                {
                    new EventLogRealTimeImporter(_time),
                    new PerformanceCounterRealTimeImporter(_time),
                    new HeartBeatRealTimeImporter(_time)
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