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
        private string _currentVersion;

        public Updater(string currentVersion) {
            _currentVersion = currentVersion;
        }

        public void Check(Uri apiUri, dynamic manifest) {
            if (_updating) {
                // Already updating
                return;
            }

            var packageUri = new Uri(apiUri, manifest.PackageUrl);
            var package = Fetch.Get(packageUri.AbsoluteUri);
            var updateDir = FileSystem.BuildAssemblyPath("Update");
            var filePath = Path.Combine(updateDir, packageUri.Segments.Last());

            if (package == null) return;

            if (!filePath.EndsWith(".zip")) {
                throw new Exception("Package URL is not a zip file.");
            }

            // Clean up failed attempts.
            if (Directory.Exists(updateDir)) {
                try { Directory.Delete(updateDir, true); } catch (IOException) {
                    return;
                }
            }

            FileSystem.AssertDirectoryExists(updateDir);
            File.WriteAllBytes(filePath, package);

            var checksum = manifest.CheckSum;

            string md5sum = FileSystem.MD5Sum(filePath);

            if (!md5sum.Equals(checksum, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("Checksums do not match on update.");
            }

            _updating = true;
            Update(filePath, updateDir);
            _updating = false;
        }

        private void Update(string packagePath, string updateDir) {
            ZipFile.ExtractToDirectory(packagePath, updateDir);
            File.Delete(packagePath);

            // Change the currently running executable so it can be overwritten.
            //Process thisprocess = Process.GetCurrentProcess();
            //string cp = thisprocess.MainModule.FileName;
            //string bak = cp + ".bak";

            //File.Move(cp, bak);
            //File.Copy(bak, cp);

            //// Copy everything.
            var assemblyPath = FileSystem.BuildAssemblyPath("");

            foreach (string file in Directory.EnumerateFiles(updateDir, "*.*")) {
                var existingPath = Path.Combine(assemblyPath, Path.GetFileName(file));

                if (File.Exists(existingPath)) {
                    var backup = existingPath + ".bak";

                    if (File.Exists(backup)) {
                        File.Delete(backup);
                    }

                    File.Copy(existingPath, backup);
                    File.Move(file, existingPath);
                }
            }

            //// Clean up.
            //Directory.Delete(WorkPath, true);

            //// Restart.
            //var spawn = Process.Start(cp);

            //thisprocess.CloseMainWindow();
            //thisprocess.Close();
            //thisprocess.Dispose();
        }
    }
}
