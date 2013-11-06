using System;
using System.Reactive.Linq;
using System.Threading;
using CSNosey.Importers;
using CSNosey.RealTimeImporters;
using PlainElastic.Net;

namespace CSNosey
{
    internal class Program
    {
        private static AutoResetEvent block = new AutoResetEvent(false);

        private static void Main(string[] args)
        {
            var connection = new ElasticConnection("asnav-monitor-01");

            Console.WriteLine(DateTime.Now);

            IRealTimeImporter importer = new EventLogRealTimeImporter();
            IRealTimeImporter importer2 = new PerformanceCounterRealTimeImporter();
            importer.Begin(connection);
            importer2.Begin(connection);

//            IObservable<DateTime> timeInterval =
//                Observable.Interval(TimeSpan.FromSeconds(1)).Select(l => DateTime.Now).Delay(TimeSpan.FromSeconds(10));

//            IImporter eventLogImporter = new EventLogImporter(connection);

//            using (timeInterval.Subscribe(eventLogImporter.Import))
//            {
//                Console.WriteLine("Press any key to unsubscribe");
//                Console.ReadKey();
//            }

//            Console.WriteLine(DateTime.Now);
//            Console.WriteLine("exit");

            block.WaitOne();
        }
    }
}

