using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using CSNosey.RealTimeImporters;
using Topshelf;

namespace CSNosey
{
    internal class AutomaticUpdater
    {
        private readonly ITime _time;
        private Version _currentVersion;
        private IDisposable _disposable;

        public AutomaticUpdater(ITime time)
        {
            _time = time;
        }

        public bool Start(HostControl control, IDbLogger connection)
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["automaticUpdate"]))
            {
                return true;
            }

            var fileVersionString = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), ".semver"));
            _currentVersion = new SemVerReader(fileVersionString).GetVersion();

            bool isProcessing = false;

            _disposable = Observable.Interval(TimeSpan.FromMinutes(1)).
                SkipWhile(l => isProcessing).
                Subscribe(l =>
                {
                    isProcessing = true;
                    try
                    {
                        var client = new WebClient();
                        var versionString = client.DownloadString(ConfigurationManager.AppSettings["versionFileAddress"]);
                        var newVersion = new SemVerReader(versionString).GetVersion();
                        
                        if (newVersion > _currentVersion)
                        {
                            connection.Log(new Event
                                {
                                    Date = _time.Now,
                                    EventId = 5384,
                                    EventRecordId = 5384,
                                    LogName = "Application",
                                    Message = string.Format("Upgrading from {0} to {1}...", _currentVersion, newVersion),
                                    Source = "Nosey",
                                    MachineName = Environment.MachineName,
                                    Level = "Information"
                                });

                            var updatePackage = new UpdatePackage(newVersion);
                            updatePackage.GetAndUnpackLatestVersion();
                            updatePackage.ReplaceUpdateScript();
                            updatePackage.RunUpdateScript();
                            StopServiceForUpdating(control);
                        }
                    }
                    catch (Exception)
                    {
                        isProcessing = false;
                    }
                });

            return true;
        }

        private static void StopServiceForUpdating(HostControl control)
        {
            control.Stop();
        }

        public bool Stop()
        {
            if (_disposable != null)
                _disposable.Dispose();
            return true;
        }
    }
}