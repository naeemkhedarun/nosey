using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Topshelf;

namespace CSNosey
{
    internal class Program
    {
        public static void Main(string[] args)
        {
//            Debugger.Launch();
            HostFactory.Run(x =>
            {
//                    x.Service<PutImportersOnTopshelf>(s =>
//                        {
//                            s.ConstructUsing(name => new PutImportersOnTopshelf());
//                            s.WhenStarted(tc => tc.Start());
//                            s.WhenStopped(tc => tc.Stop());
//                        });

                x.Service<AutomaticUpdater>(s =>
                {
                    s.ConstructUsing(settings => new AutomaticUpdater());
                    s.WhenStarted((updater, control) => updater.Start(control));
                    s.WhenStopped((updater, control) => updater.Stop());
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

    internal class AutomaticUpdater
    {
        private Version _currentVersion;
        private IDisposable _disposable;

        public bool Start(HostControl control)
        {
            var fileVersionString = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ".semver"));
            _currentVersion = new SemVerReader(fileVersionString).GetVersion();

            bool isProcessing = false;

            _disposable = Observable.Interval(TimeSpan.FromSeconds(5)).
                Where(i => !isProcessing).
                Subscribe(l =>
                {
                    isProcessing = true;
                    try
                    {
                        var client = new WebClient();
                        var versionString = client.DownloadString("http://localhost/agent/.semver");
//                        var versionString = client.DownloadString("https://raw.github.com/naeemkhedarun/nosey/master/releases/agent/.semver");
                        var version = new SemVerReader(versionString).GetVersion();

                        if (version > _currentVersion)
                        {
                            Console.WriteLine("version updated to " + version.ToString());
                            UpdateUpdater();
                            var updateScript = Path.Combine(Directory.GetCurrentDirectory(), "Update-Version.ps1");
                            var userName = string.Format("-NoProfile -NoLogo -NoExit -File {0}", updateScript);
                            Process.Start("powershell.exe", userName);
                            control.Stop();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        isProcessing = false;
                    }
                });

            return true;
        }

        private void UpdateUpdater()
        {
            var client = new WebClient();
            client.DownloadFile("http://localhost/Update-Version.ps1", "Update-Version.ps1");
//            client.DownloadFile("https://github.com/naeemkhedarun/nosey/blob/master/releases/agent/Update-Version.ps1", "Update-Version.ps1");
        }

        public bool Stop()
        {
            _disposable.Dispose();
            return true;
        }
    }
}