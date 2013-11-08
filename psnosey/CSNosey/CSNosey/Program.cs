using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using CSNosey.Importers;
using CSNosey.RealTimeImporters;
using PlainElastic.Net;
using Topshelf;

namespace CSNosey
{
    class PutImportersOnTopshelf
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

            foreach (var realTimeImporter in _importers)
            {
                realTimeImporter.Begin(_connection);
            }
        }

        public void Stop()
        {
            foreach (var realTimeImporter in _importers)
            {
                ((IDisposable) realTimeImporter).Dispose();
            }

        }
    }

    internal class Program
    {
        private static AutoResetEvent block = new AutoResetEvent(false);

        private static void Main(string[] args)
        {
            HostFactory.Run(x =>                                 //1
            {
                x.Service<PutImportersOnTopshelf>(s =>                        //2
                {
                    s.ConstructUsing(name => new PutImportersOnTopshelf());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("Collects machine data and pushes to elastic search");        //7
                x.SetDisplayName("Nosey");                       //8
                x.SetServiceName("Nosey");                       //9
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

