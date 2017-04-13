using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PTMS.Core.Api;
using PTMS.Core.Configuration;
using PTMS.Core.Utilities;

namespace PTMSController {
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window {
        protected Version ServerVersion;
        protected Version Version;
        private dynamic _creds;
        private dynamic _manifest;


        public UpdateWindow() {
            InitializeComponent();

            _creds = Utilities.GetCredentials();
            _manifest = DashboardConnector.GetUpdateManifest(_creds.ApiUri, ServerVersion.ToString(), _creds.AuthToken);
            ServerVersion = new Version(DashboardConnector.GetVersion(_creds.ApiUri, _creds.AuthToken).Version);
            Version = Assembly.GetExecutingAssembly().GetName().Version;

            lblVersion.Content = Version;

            if (ServerVersion.CompareTo(Version) > 0) {
                lblServerVersion.Content = String.Format("The lastest verision is {0}", ServerVersion);
                btnUpdate.Visibility = Visibility.Visible;
            }

        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            try {
                // Do update

                Check(_creds.ApiUri, _manifest);
            } catch (Exception ex) {
                MessageBox.Show("Unable to check for updates.  Please try again shortly.", "Update", MessageBoxButton.OK);
            }
        }

        private void startDownload(Uri uri, string writePath) {
            Thread thread = new Thread(() => {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += client_DownloadProgressChanged;
                client.DownloadFileCompleted += client_DownloadFileCompleted;
                client.DownloadFileAsync(uri, writePath);
            });
            thread.Start();
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            App.Current.Dispatcher.Invoke(delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;

                btnUpdate.Content = String.Format("Downloaded {0:P}", percentage);
                ProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            App.Current.Dispatcher.Invoke(delegate {
                btnUpdate.Content = "Completed";
            });

            var packageUri = new Uri(_creds.ApiUri, _manifest.PackageUrl);
            var updateDir = FileSystem.BuildAssemblyPath("Update");
            var filePath = Path.Combine(updateDir, packageUri.Segments.Last());

            var checksum = _manifest.CheckSum;

            string md5sum = FileSystem.MD5Sum(filePath);

            if (!md5sum.Equals(checksum, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("Checksums do not match on update.");
            }

            Updater.Update(filePath, updateDir);
        }

        public void Check(Uri apiUri, dynamic manifest) {            
            var packageUri = new Uri(apiUri, manifest.PackageUrl);
            var updateDir = FileSystem.BuildAssemblyPath("Update");
            var filePath = Path.Combine(updateDir, packageUri.Segments.Last());

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
            startDownload(new Uri(packageUri.AbsoluteUri), filePath);


        }

    }
}
