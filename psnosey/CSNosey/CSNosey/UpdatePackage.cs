using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace CSNosey
{
    internal class UpdatePackage
    {
        private readonly Version _newVersion;
        private string _updatePackageFullPath;

        public UpdatePackage(Version newVersion)
        {
            _newVersion = newVersion;
        }

        public void GetAndUnpackLatestVersion()
        {
            var client = new WebClient();
            string usersTempFolder = Path.GetTempPath();

            string updateZipName = string.Format("v{0}.zip", _newVersion);

            string updateZipLocation = Path.Combine(usersTempFolder, updateZipName);

            client.DownloadFile(Path.Combine(ConfigurationManager.AppSettings["updatePackageAddress"], updateZipName),
                updateZipLocation);

            string updatePackageFullPath = Path.Combine(usersTempFolder, _newVersion.ToString());

            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(updateZipLocation);
                zf = new ZipFile(fs);

                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;
                    }

                    String entryFileName = zipEntry.Name;

                    var buffer = new byte[4096];
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    String fullZipToPath = Path.Combine(updatePackageFullPath, Path.GetFileName(entryFileName));
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true;
                    zf.Close();
                }
            }

            _updatePackageFullPath = updatePackageFullPath;
        }

        public void ReplaceUpdateScript()
        {
            if (!bool.Parse(ConfigurationManager.AppSettings["IgnoreUpdatingUpdateScript"]))
            {
                string updateScriptName = ConfigurationManager.AppSettings["UpdateScriptName"];
                string sourceFilePath = Path.Combine(_updatePackageFullPath, updateScriptName);
                string targetFilePath = Path.Combine(Directory.GetCurrentDirectory(), updateScriptName);
                File.Copy(sourceFilePath, targetFilePath, true);
            }
        }

        public void RunUpdateScript()
        {
            string updateScriptName = ConfigurationManager.AppSettings["UpdateScriptName"];
            string scriptFullPath = Path.Combine(Directory.GetCurrentDirectory(), updateScriptName);
            string userName = string.Format("-NoProfile -NoLogo -NoExit -File {0} {1} {2}", scriptFullPath,
                _updatePackageFullPath, Directory.GetCurrentDirectory());
            Process.Start("powershell.exe", userName);
        }
    }
}