using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using Topshelf;

namespace CSNosey
{
    internal class AutomaticUpdater
    {
        private Version _currentVersion;
        private IDisposable _disposable;

        public bool Start(HostControl control)
        {
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
                            var updatePackage = new UpdatePackage(newVersion);
                            updatePackage.GetAndUnpackLatestVersion();
                            updatePackage.ReplaceUpdateScript();
                            updatePackage.RunUpdateScript();
                            StopServiceForUpdating(control);
                        }
                    }
                    catch (Exception e)
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
            _disposable.Dispose();
            return true;
        }
    }
}