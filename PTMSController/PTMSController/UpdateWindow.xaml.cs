using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
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
        private dynamic _manifest;

        private readonly PracticeControllerManager pcm;

        public UpdateWindow() {
            InitializeComponent();

            pcm = PracticeControllerManager.Current;

            try {
                ServerVersion = new Version(DashboardConnector.GetVersion(pcm.ApiCredentials.ApiUri, pcm.ApiCredentials.AuthToken).Version);
                Version = Assembly.GetExecutingAssembly().GetName().Version;
                _manifest = DashboardConnector.GetUpdateManifest(pcm.ApiCredentials.ApiUri, ServerVersion.ToString(), pcm.ApiCredentials.AuthToken);

                lblVersion.Content = Version;

                if (ServerVersion.CompareTo(Version) > 0) {
                    lblServerVersion.Content = String.Format("The lastest verision is {0}", ServerVersion);
                    btnUpdate.Visibility = Visibility.Visible;
                }                
            } catch (Exception ex) {
                pcm.Logger.LogException("Error Loading Update Window", ex.ToString());
            }

        }

        private void Update_Click(object sender, RoutedEventArgs e) {
            try {
                // Do update
                ProgressBar.Visibility = Visibility.Visible;

                Check(pcm.ApiCredentials.ApiUri, _manifest);
            } catch (Exception ex) {
                MessageBox.Show("Unable to check for updates.  Please try again shortly.", "Update", MessageBoxButton.OK);
                pcm.Logger.LogException("Error Checking Updates", ex.ToString());
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

            App.Current.Dispatcher.Invoke(delegate { btnUpdate.IsEnabled = false; });
        }
        void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
            App.Current.Dispatcher.Invoke(delegate {
                double bytesIn = double.Parse(e.BytesReceived.ToString());
                double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
                double percentage = bytesIn / totalBytes * 100;

                lblServerVersion.Content = String.Format("Downloaded: {0:P}", percentage/100);
                ProgressBar.Value = int.Parse(Math.Truncate(percentage).ToString());
            });
        }
        void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
            App.Current.Dispatcher.Invoke(delegate {
                btnUpdate.Content = "Completed";
            });

            var packageUri = new Uri(pcm.ApiCredentials.ApiUri, _manifest.PackageUrl);
            var updateDir = FileSystem.BuildAssemblyRelPath("Update");
            var filePath = Path.Combine(updateDir, packageUri.Segments.Last());
            var checksum = _manifest.CheckSum;
            string md5sum = FileSystem.MD5Sum(filePath);

            if (!md5sum.Equals(checksum, StringComparison.CurrentCultureIgnoreCase)) {
                throw new Exception("Checksums do not match on update.");
            }

            Updater.Update(filePath, updateDir);

            try {
                App.Current.Dispatcher.Invoke(delegate { App.Current.Shutdown(); });
            } catch {
                MessageBox.Show("There was a problem shutting down the application. Please shut down all instances of the PTMS Service and Dashboard before continuing with the update.");
            }
            
        }

        public void Check(Uri apiUri, dynamic manifest) {            
            var packageUri = new Uri(apiUri, manifest.PackageUrl);
            var updateDir = FileSystem.BuildAssemblyRelPath("Update");
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
