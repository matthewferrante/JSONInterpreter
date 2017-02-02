using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PTMS.Core.Api;

namespace PTMS.Core.Utilities {
    public class Updater {

        private volatile bool _updating;
        private readonly Manifest _localConfig;
        private Manifest _remoteConfig;
        private readonly FileInfo _localConfigFile;


        public void Check(Uri updateUri) {
            if (_updating) {
                // Already updating
                return;
            }


            var http = new Fetch { Retries = 5, RetrySleep = 30000, Timeout = 30000 };
            http.Load(updateUri.AbsoluteUri);

            if (!http.Success) {
                _remoteConfig = null;
                return;
            }

            string data = Encoding.UTF8.GetString(http.ResponseData);
            _remoteConfig = new Manifest(data);

            if (_remoteConfig == null)
                return;

            if (_localConfig.SecurityToken != _remoteConfig.SecurityToken) {
                return;
            }

            if (_remoteConfig.Version <= _localConfig.Version) {
                return;
            }

            _updating = true;
            Update(FileSystem.BuildAssemblyPath("Update"));
            _updating = false;
        }

        private void Update(string WorkPath) {
            // Clean up failed attempts.
            if (Directory.Exists(WorkPath)) {
                try { Directory.Delete(WorkPath, true); } catch (IOException) {
                    return;
                }
            }

            FileSystem.AssertDirectoryExists(WorkPath);

            // Download files in manifest.
            foreach (string update in _remoteConfig.Payloads) {
                var url = _remoteConfig.BaseUri + update;
                var file = Fetch.Get(url);

                if (file == null) {
                    return;
                }

                var info = new FileInfo(Path.Combine(WorkPath, update));

                //Directory.CreateDirectory(info.DirectoryName);
                File.WriteAllBytes(Path.Combine(WorkPath, update), file);

                // Unzip
                if (Regex.IsMatch(update, @"\.zip")) {
                    try {
                        var zipfile = Path.Combine(WorkPath, update);

                        ZipFile.ExtractToDirectory(zipfile, WorkPath);
                        File.Delete(zipfile);
                    } catch (Exception ex) {
                        return;
                    }
                }
            }

            // Change the currently running executable so it can be overwritten.
            Process thisprocess = Process.GetCurrentProcess();
            string cp = thisprocess.MainModule.FileName;
            string bak = cp + ".bak";

            if (File.Exists(bak)) {
                File.Delete(bak);
            }
                
            File.Move(cp, bak);
            File.Copy(bak, cp);

            // Write out the new manifest.
            _remoteConfig.Write(Path.Combine(WorkPath, _localConfigFile.Name));

            // Copy everything.
            var directory = new DirectoryInfo(WorkPath);
            var files = directory.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (FileInfo file in files) {
                string destination = file.FullName.Replace(directory.FullName + @"\", "");
                Directory.CreateDirectory(new FileInfo(destination).DirectoryName);
                file.CopyTo(destination, true);
            }

            // Clean up.
            Directory.Delete(WorkPath, true);

            // Restart.
            var spawn = Process.Start(cp);

            thisprocess.CloseMainWindow();
            thisprocess.Close();
            thisprocess.Dispose();
        }
    }
}
