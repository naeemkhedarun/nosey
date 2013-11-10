using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Topshelf;

namespace CSNosey
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<PutImportersOnTopshelf>(s =>
                    {
                        s.ConstructUsing(name => new PutImportersOnTopshelf());
                        s.WhenStarted((topshelf, control) => topshelf.Start(control));
                        s.WhenStopped(tc => tc.Stop());
                    });

                x.RunAsLocalService();
                
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
        }
    }
}