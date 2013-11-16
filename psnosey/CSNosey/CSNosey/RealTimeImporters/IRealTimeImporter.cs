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
        private IDisposable _disposable;

        public void Begin(IDbLogger connection)
        {
            _disposable = Observable.Interval(TimeSpan.FromSeconds(10)).Subscribe(l => connection.UpdateHeatbeat(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss")));
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}