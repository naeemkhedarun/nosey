using System;
using System.Reactive.Linq;

namespace CSNosey.RealTimeImporters
{
    internal interface IRealTimeImporter
    {
        void Begin(IDbLogger connection);
    }

    class HeartBeatRealTimeImporter : IRealTimeImporter, IDisposable
    {
        private readonly ITime _time;
        private IDisposable _disposable;

        public HeartBeatRealTimeImporter(ITime time)
        {
            _time = time;
        }

        public void Begin(IDbLogger connection)
        {
            _disposable = Observable.Interval(TimeSpan.FromSeconds(10)).Subscribe(l => connection.UpdateHeatbeat(_time.Now));
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}