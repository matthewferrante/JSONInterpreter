using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using PTMS.Core;
using System.Configuration;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using PTMS.Core.Api;
using PTMS.Core.Configuration;
using PTMS.Core.Logging;
using PTMS.Core.Utilities;
using PTMSController.Models;

namespace PTMSController {
    public partial class MainWindow {
        private const string
            SERVICE_INSTALL = "Install Service",
            SERVICE_START = "Start",
            SERVICE_STOP = "Stop";

        private LogWatcher _logWatcher;
        private FileSystemWatcher _incomingWatcher;
        private bool _isInstalled = false;
        private Logger _logger;
        private ObservableCollection<ReviewRow> _reviewRows = new ObservableCollection<ReviewRow>();
        private PracticeControllerManager Manager { get { return PracticeControllerManager.Current; }}

        public MainWindow() {
            _logger = new Logger() {
                NameSpace = Constants.DASHBOARD_NAME
            };

            // Set the Singleton of the Manager
            Application.Current.Properties.Add(Constants.CONTROLLER_MANAGER, new PracticeControllerManager(_logger));

            InitializeComponent();
            Setup();
            CreateWatchers();
        }

        public void Setup() {
            InitButton();
            InitDirectories();

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, a) => { InitButton(); };
            timer.Start();

            dgPersons.ItemsSource = _reviewRows;
        }

        public void ShowDashboard(object sender, RoutedEventArgs e) {
            GroupHome.Visibility = Visibility.Visible;
            RTBLogWindow.Visibility = Visibility.Hidden;
        }
        public void ShowLog(object sender, RoutedEventArgs e) {
            GroupHome.Visibility = Visibility.Hidden;
            RTBLogWindow.Visibility = Visibility.Visible;            
        }

        /// <summary>
        /// Installs the service.  
        /// </summary>
        /// <param name="uninstall"></param>
        public void InstallService(bool uninstall = false) {
            string relPath = Utilities.GetSetting(Constants.SERVICE_PATH_KEY);
            string rawName = Assembly.GetExecutingAssembly().Location;
            string dirName = Path.GetDirectoryName(rawName) ?? ".\\";
            string fileName = Path.Combine(dirName, relPath, Constants.SERVICE_EXE);

            try {
                if (uninstall) {
                    MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to uninstall the service?", "Uninstall Confirmation", MessageBoxButton.YesNo);

                    if (messageBoxResult == MessageBoxResult.Yes) {
                        ManagedInstallerClass.InstallHelper(new[] { "/u", fileName });
                        _isInstalled = false;
                    }
                } else {
                    ManagedInstallerClass.InstallHelper(new[] { fileName });
                    _isInstalled = true;
                }
            } catch (Exception ex) {
                MessageBox.Show(String.Format("There was an error installing the service.\n\nException: {0}", ex));
            }

            try {
                CreateWatchers();
            } catch {
                // ignored
            }
        }

        private void CreateWatchers() {
            CreateLogWatcher();
            CreateIncomingWatcher();
        }
        private void CreateLogWatcher() {
            if (_isInstalled) {
                string logFile = FileSystem.BuildAssemblyPath(ConfigurationManager.AppSettings[Constants.SERVICE_PATH_KEY]) + ConfigurationManager.AppSettings[Constants.APP_SETTING_LOGFILE];


                try {
                    // Initial load of the window.  Put the last 10 lines of text in the box for users to view.
                    RTBLogWindow.AppendText(String.Join("\n", File.ReadLines(logFile).Reverse().Take(10).Reverse()) + "\n");
                } catch {
                    _logger.Log(String.Format("Can't find the log file.  The log file will be recreated at {0}.", logFile));
                    File.Create(logFile);
                }

                // Raise events when the LastWrite or Size attribute is changed
                // Filter out events for only this file
                _logWatcher = new LogWatcher(logFile) {
                    NotifyFilter = (NotifyFilters.LastWrite | NotifyFilters.Size),
                    Filter = Path.GetFileName(logFile)
                };

                _logWatcher.TextChanged += Watcher_Changed; // Subscribe to the event
                _logWatcher.EnableRaisingEvents = true; // Enable the event
            }
        }
        private void CreateIncomingWatcher() {
            _incomingWatcher = new FileSystemWatcher() {
                Path = Manager.IncomingDirectory,
                Filter = "*.json",
                EnableRaisingEvents = true
            };

            _incomingWatcher.Created += ProcessIncoming;
            _incomingWatcher.Deleted += ProcessIncoming;
        }


        /// <summary>
        /// Controls when the button is clicked and what should happen in that case.  If the service is stopped it will issue the start command.  
        /// If the service is started it will issue the stop command.  If the service is not installed, it will install the service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnServiceControl_Click(object sender, RoutedEventArgs e) {
            var sc = GetService();
            BtnServiceControl.IsEnabled = false;

            switch (BtnServiceControl.Content.ToString()) {
                case SERVICE_START:
                    sc.Start();
                    break;
                case SERVICE_STOP:
                    sc.Stop();
                    break;
                case SERVICE_INSTALL:
                    InstallService();
                    break;
            }
        }
        private void btnSendAll_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("Are you sure you want to send all charts?", "Confrim Send All", MessageBoxButton.YesNo);

            _logger.Log("Send All Charts Initiated.");

            if (result == MessageBoxResult.Yes) {
                foreach (var row in _reviewRows) {
                    var newPath = Manager.ProcessedDirectory + Path.GetFileName(row.FileName);
                    if (File.Exists(newPath)) {
                        File.Delete(newPath);
                    }

                    File.Move(row.FileName, newPath);
                }
            }
        }
        private void btnGetFiles_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("Are you sure you want to download charts now?", "Confirm Download", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes) {
                _logger.Log("Dashboard Downloading Files");

                var practiceId = Utilities.GetSetting(Constants.SETTING_PRACTICE_ID);

                try {
                    PracticeConnector.DownloadReports(Utilities.GetCredentials(), practiceId, _logger, Manager.IncomingDirectory);

                    MessageBox.Show("Files successfully downloaded.", "Success", MessageBoxButton.OK);
                } catch {
                    MessageBox.Show("The host server is currently unavailable.  Please try again in a few minutes.", "Server Error", MessageBoxButton.OK);
                }
            }
        }

        private ServiceController GetService() {
            return ServiceController.GetServices().FirstOrDefault(x => x.ServiceName.Equals(Constants.SERVICE_NAME));
        }

        /// <summary>
        /// Initialize the button state
        /// </summary>
        private void InitButton() {
            ServiceController sc = GetService();
            _isInstalled = sc != null;
            var status = (sc != null) ? sc.Status.ToString() : "Not Installed.";

            TblkServiceStatus.Text = Constants.SERVICE_NAME + " is currently " + status;

            if (!_isInstalled) {
                BtnServiceControl.Content = SERVICE_INSTALL;
                BtnServiceControl.IsEnabled = true;
            } else {
                switch (sc.Status) {
                    case ServiceControllerStatus.ContinuePending:
                    case ServiceControllerStatus.StopPending:
                    case ServiceControllerStatus.PausePending:
                    case ServiceControllerStatus.StartPending:
                        BtnServiceControl.IsEnabled = false;
                        break;
                    case ServiceControllerStatus.Stopped:
                    case ServiceControllerStatus.Paused:
                        BtnServiceControl.IsEnabled = true;
                        BtnServiceControl.Content = SERVICE_START;
                        break;
                    case ServiceControllerStatus.Running:
                        BtnServiceControl.IsEnabled = true;
                        BtnServiceControl.Content = SERVICE_STOP;
                        break;
                }
            }

        }

        /// <summary>
        /// Initialize all the directories used by the system.  If the folders do not exist go ahead and create them to avoid file errors.
        /// </summary>
        private void InitDirectories() {
            ProcessIncoming();
        }

        /// <summary>
        /// Close the application
        /// </summary>
        /// <param name="sender">Object initiating the close operation</param>
        /// <param name="e">Event Parameters</param>
        private void ExitClick(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void UninstallClick(object sender, RoutedEventArgs e) {
            InstallService(true); // Un-install the service
        }

        /// <summary>
        /// Occurs when the file is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(object sender, LogWatcherEventArgs e) {
            //Invoke the AppendText method if required
            Dispatcher.Invoke(delegate() {
                RTBLogWindow.AppendText(e.Contents);
                RTBLogWindow.ScrollToEnd();
            });
        }

        private void ProcessIncoming() {
            try {
                App.Current.Dispatcher.Invoke(delegate { _reviewRows.Clear(); });

                foreach (string file in Directory.EnumerateFiles(Manager.IncomingDirectory, "*.json")) {
                    try {
                        string contents = File.ReadAllText(file);

                        dynamic r = JObject.Parse(contents);
                        var dob = String.Format("{0}/{1}/{2}", r.Patient.DateOfBirth.Month, r.Patient.DateOfBirth.Day, r.Patient.DateOfBirth.Year);

                        App.Current.Dispatcher.Invoke(delegate { _reviewRows.Add(new ReviewRow() { Id = r.Patient.PatientId, FirstName = r.Patient.FirstName, LastName = r.Patient.LastName, DOB = dob, Gender = r.Patient.Gender, FileName = file }); });                        
                    } catch (Exception ex) {
                        _logger.Log(String.Format("Error processing incoming file: {0}\n***************************************** EXCEPTION ********************************************\n{1}\n************************************************************************\n", file, ex));
                    }
                }
            } catch (Exception exception) {
                _logger.Log(String.Format("Exception Processing Incoming: {0}", exception));
            }            
        }
        private void ProcessIncoming(object sender, FileSystemEventArgs e) {
            ProcessIncoming();
        }

        private void ReviewPatient(object sender, RoutedEventArgs e) {
            var reviewRow = (sender as Button).DataContext as ReviewRow;

            var reviewWindow = new ReviewWindow(reviewRow);
            reviewWindow.ShowDialog();
        }
        private void DeleteIncoming(object sender, RoutedEventArgs e) {
            //var creds = Utilities.GetCredentials();

            try {
                var reviewRow = (sender as Button).DataContext as ReviewRow;
                var fn = Path.GetFileName(reviewRow.FileName);

                Manager.DeleteIncomingQuestionnaire(fn);
            } catch (NullReferenceException nre) {
                MessageBox.Show(String.Format("And Error occurred removing report.  Error: {0}", nre.Message));
            }
        }

        private void ShowAbout(object sender, EventArgs e) {
            MessageBox.Show(String.Format("\u00a9 {0} Primetime Medical Software\nCurrent Version: {1}", DateTime.Now.Year, Assembly.GetExecutingAssembly().GetName().Version), "About", MessageBoxButton.OK);
        }
        private void CheckUpdates(object sender, EventArgs e) {
            var creds = Utilities.GetCredentials();
            try {
                var v = DashboardConnector.GetVersion(creds.ApiUri, creds.AuthToken);

                var s = String.Format("Current Version: {0}\nLastest Version: {1}", Assembly.GetExecutingAssembly().GetName().Version, v.Version);

                MessageBox.Show(s, "Update", MessageBoxButton.OK);                
            } catch (Exception ex) {
                MessageBox.Show("Unable to check for updates.  Please try again shortly.", "Update", MessageBoxButton.OK);
                
                _logger.LogException("Check Updates",ex.ToString());
            }
        }
    }
}
