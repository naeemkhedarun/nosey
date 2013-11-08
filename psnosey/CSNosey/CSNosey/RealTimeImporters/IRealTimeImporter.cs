using System;
using System.Reactive.Linq;
using PlainElastic.Net;
using PlainElastic.Net.Serialization;

namespace CSNosey.RealTimeImporters
{
    internal interface IRealTimeImporter
    {
        void Begin(ElasticConnection connection);
    }

    class HeartBeatRealTimeImporter : IRealTimeImporter, IDisposable
    {
        private IDisposable _disposable;

        public void Begin(ElasticConnection connection)
        {
            var jsonNetSerializer = new JsonNetSerializer();

            _disposable = Observable.Interval(TimeSpan.FromSeconds(10)).Subscribe(l =>
                {
                    var serialize = jsonNetSerializer.Serialize(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"));
                    connection.Put(new IndexCommand("counter", "heartbeat", Environment.MachineName), serialize);
                });
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}