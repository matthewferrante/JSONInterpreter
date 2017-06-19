using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Net;
using System.Threading;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using PTMS.Core.Api;
using PTMS.Core.Configuration;
using PTMS.Core.Crypto;
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
        private PracticeControllerManager Manager { get { return PracticeControllerManager.Current; } }

        public MainWindow() {
            try {

                _logger = new Logger() {
                    NameSpace = Constants.DASHBOARD_NAME
                };

                // Set the Singleton of the Manager
                Application.Current.Properties.Add(Constants.CONTROLLER_MANAGER, new PracticeControllerManager(_logger));

                InitializeComponent();
                Setup();
                CreateWatchers();
            } catch (ConfigurationErrorsException cee) {
                MessageBox.Show(String.Format("A configuration error has occurred that has prevented the program from starting.  Please check the config file and try again.\n{0}",cee));
                Application.Current.Shutdown();
            }
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
                string logFile = FileSystem.BuildAssemblyRelPath(ConfigurationManager.AppSettings[Constants.SERVICE_PATH_KEY]) + ConfigurationManager.AppSettings[Constants.APP_SETTING_LOGFILE];


                try {
                    // Initial load of the window.  Put the last 10 lines of text in the box for users to view.
                    RTBLogWindow.AppendText(String.Join("\n", File.ReadLines(logFile).Reverse().Take(10).Reverse()) + "\n");
                } catch {
                    Manager.Logger.Log(String.Format("Can't find the log file.  The log file will be recreated at {0}.", logFile));
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

            if (sc == null) {
                Manager.Logger.LogException("SERVICE CONTROL", "Unable to get Service.");
            }

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
            var result = MessageBox.Show("Are you sure you want to send all charts?", "Confirm Send All", MessageBoxButton.YesNo);
            Manager.Logger.Log("Send All Charts Initiated.");


            try {

                if (result == MessageBoxResult.Yes) {
                    var key = PracticeConnector.GetEncryptionKey(Manager.ApiCredentials.ApiUri, Manager.ApiCredentials.AuthToken);

                    foreach (var row in _reviewRows) {
                        var newPath = Path.Combine(Manager.ProcessedDirectory, Path.GetFileName(row.FileName));

                        if (File.Exists(newPath)) {
                            File.Delete(newPath);
                        }

                        var text = File.ReadAllText(row.FileName);

                        try {
                            File.WriteAllText(newPath, StringCipher.Decrypt(text, key));
                            File.Delete(row.FileName);
                        } catch (Exception ex) {
                            Manager.Logger.LogException("Send All:Write File", ex.ToString());
                        }
                    }
                }
            } catch (Exception ex) {
              Manager.Logger.LogException("Error Sending All Charts",ex.ToString());  
            }
        }
        private void btnGetFiles_Click(object sender, RoutedEventArgs e) {
            var result = MessageBox.Show("Are you sure you want to download charts now?", "Confirm Download", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes) {
                Manager.Logger.Log("Dashboard Downloading Files");

                try {
                    var key = PracticeConnector.GetEncryptionKey(Manager.ApiCredentials.ApiUri, Manager.ApiCredentials.AuthToken);

                    PracticeConnector.DownloadReports(Manager.ApiCredentials, _logger, Manager.IncomingDirectory, key);

                    MessageBox.Show("Files successfully downloaded.", "Success", MessageBoxButton.OK);
                    Manager.Logger.Log("Successfully downloaded reports. [Manual]");
                } catch {
                    MessageBox.Show("The host server is currently unavailable.  Please try again in a few minutes.", "Server Error", MessageBoxButton.OK);
                    Manager.Logger.Log("Unable to download reports due to server failure. [Manual]");
                }
            }
        }

        private ServiceController GetService() {
            return ServiceController.GetServices().FirstOrDefault(x => x.ServiceName.Equals(Constants.SERVICE_NAME));
        }

        /// <summary>
        /// Initialize the button state for service control
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
            Dispatcher.Invoke(delegate() {
                RTBLogWindow.AppendText(e.Contents); // append new contents to the Log Window
                RTBLogWindow.ScrollToEnd();
            });
        }

        private void ProcessIncoming() {
            try {
                App.Current.Dispatcher.Invoke(delegate { _reviewRows.Clear(); });

                var key = PracticeConnector.GetEncryptionKey(Manager.ApiCredentials.ApiUri, Manager.ApiCredentials.AuthToken);

                foreach (string file in Directory.EnumerateFiles(Manager.IncomingDirectory, "*.json")) {
                    try {
                        string contents = StringCipher.Decrypt(File.ReadAllText(file),key);
                        dynamic r = JObject.Parse(contents);
                        var dob = String.Format("{0}/{1}/{2}", r.Patient.DateOfBirth.Month, r.Patient.DateOfBirth.Day, r.Patient.DateOfBirth.Year);

                        App.Current.Dispatcher.Invoke(delegate { _reviewRows.Add(new ReviewRow() { Id = r.Patient.PatientId, FirstName = r.Patient.FirstName, LastName = r.Patient.LastName, DOB = dob, Gender = r.Patient.Gender, FileName = file }); });  // Add Row to current display               
                    } catch (Exception ex) {
                        Manager.Logger.LogException(String.Format("Processing Incoming: {0}", file), ex.ToString());
                    }
                }
            } catch (Exception exception) {
                Manager.Logger.LogException("Exception Processing Incoming", exception.ToString());
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

        private void CheckUpdates(object sender, RoutedEventArgs e) {
            var updateWindow = new UpdateWindow();
            updateWindow.ShowDialog();
        }
        private void DeleteIncoming(object sender, RoutedEventArgs e) {
            try {
                var reviewRow = (sender as Button).DataContext as ReviewRow;
                var fn = Path.GetFileName(reviewRow.FileName);

                Manager.DeleteIncomingQuestionnaire(fn);
            } catch (NullReferenceException nre) {
                MessageBox.Show(String.Format("And Error occurred removing report.  Error: {0}", nre.Message));
                Manager.Logger.LogException("Delete Incoming", nre.ToString());
            }
        }

        private void ShowAbout(object sender, EventArgs e) {
            MessageBox.Show(String.Format("\u00a9 {0} Primetime Medical Software\nCurrent Version: {1}", DateTime.Now.Year, Assembly.GetExecutingAssembly().GetName().Version), "About", MessageBoxButton.OK);
        }


        private void Test_Click(object sender, EventArgs e) {
            //var creds = Utilities.GetCredentials();

            var password = PracticeConnector.GetEncryptionKey(Manager.ApiCredentials.ApiUri, Manager.ApiCredentials.AuthToken);
            //var input = File.ReadAllText(_reviewRows[0].FileName);

            var input = "This is a test string";

            var sc = StringCipher.Encrypt(input, password);

            MessageBox.Show(String.Format("encrypted = {0}", sc));

            var dc = StringCipher.Decrypt(sc, password);

            MessageBox.Show(String.Format("decrypted = {0}", dc));

        }
    }
}
