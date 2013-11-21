using CSNosey.RealTimeImporters;
using Topshelf;

namespace CSNosey
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                var time = new Time();

                x.Service<PutImportersOnTopshelf>(s =>
                    {
                        s.ConstructUsing(name => new PutImportersOnTopshelf(time));
                        s.WhenStarted((topshelf, control) => topshelf.Start(control));
                        s.WhenStopped(tc => tc.Stop());
                        x.StartAutomatically(); 
                    });

                x.RunAsLocalService();
                
                x.SetDescription("Collects machine data and pushes to elastic search");
                x.SetDisplayName("Nosey");
                x.SetServiceName("Nosey");
            });
        }
    }
}