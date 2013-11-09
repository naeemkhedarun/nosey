using System;
using System.Collections.Generic;
using System.Threading;
using CSNosey.RealTimeImporters;
using PlainElastic.Net;
using Topshelf;

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

    internal class Program
    {
        private static readonly AutoResetEvent block = new AutoResetEvent(false);

        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
                {
                    x.Service<PutImportersOnTopshelf>(s =>
                        {
                            s.ConstructUsing(name => new PutImportersOnTopshelf());
                            s.WhenStarted(tc => tc.Start());
                            s.WhenStopped(tc => tc.Stop());
                        });
                    x.RunAsLocalSystem();

                    x.SetDescription("Collects machine data and pushes to elastic search");
                    x.SetDisplayName("Nosey");
                    x.SetServiceName("Nosey");
                });


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