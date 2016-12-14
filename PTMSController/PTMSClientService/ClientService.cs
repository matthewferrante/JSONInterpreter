using PTMS.Core;
using PTMS.Core.Logging;
using System;
using System.Configuration;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PTMS.Core.Api;
using PTMS.Core.Utilities;
using System.IO;
using PTMS.Core.Configuration;

namespace PTMSClientService {
    public partial class ClientService : ServiceBase {
        private Timer _reportTimer;
        private Timer _updateTimer;
        private Timer _uploadTimer;
        private Logger _logger;
        private string _incomingDir;
        private string _outgoingDir;
        private string _uploadDir;
        private string _practiceId;


        public ClientService() {
            _logger = new Logger();
            _logger.NameSpace = Constants.SERVICE_NAME;
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _logger.Log("Service Started");

            _incomingDir = FileSystem.BuildAssemblyPath(ConfigurationManager.AppSettings[Constants.SETTING_INCOMING_DIRECTORY]);
            _outgoingDir = FileSystem.BuildAssemblyPath(ConfigurationManager.AppSettings[Constants.SETTING_OUTGOING_DIRECTORY]);
            _practiceId = ConfigurationManager.AppSettings[Constants.SETTING_PRACTICE_ID];

            FileSystem.AssertDirectoryExists(_outgoingDir);
            FileSystem.AssertDirectoryExists(_incomingDir);

            ScheduleServices();
        }

        protected override void OnStop() {
            _reportTimer.Dispose();
            _updateTimer.Dispose();
            _logger.Log("Service Stopped");
        }

        public void ScheduleServices() {
            ScheduleReportTimer();
            ScheduleUpdateTimer();
            ScheduleDataUploader();
        }

        public void ScheduleReportTimer() {
            ScheduleTimer(ref _reportTimer, ReportTimerCallback, Convert.ToInt32(ConfigurationManager.AppSettings[Constants.INTERVAL_REPORT]), "Report");
        }
        public void ScheduleUpdateTimer() {
            ScheduleTimer(ref _updateTimer, UpdateTimerCallback, Convert.ToInt32(ConfigurationManager.AppSettings[Constants.INTERVAL_UPDATE]), "Update");
        }
        private void ScheduleDataUploader() {
            ScheduleTimer(ref _uploadTimer, UploadTimerCallback, Convert.ToInt32(ConfigurationManager.AppSettings[Constants.INTERVAL_UPLOAD]), "Upload");
        }

        private void UploadTimerCallback(object e) {
            var creds = Utilities.GetCredentials();

            foreach (string file in Directory.EnumerateFiles(_outgoingDir, "*.*")) {
                try {
                    var b = PracticeConnector.SendFile(creds.ApiUri, _practiceId, file, creds.AuthToken);

                    if (b.Result) {
                        _logger.Log((String.Format("Successfully Transmitted: {0}", file)));

                        FileSystem.AssertDirectoryExists(Path.Combine(_outgoingDir, "Sent"));
                        File.Move(file, Path.Combine(_outgoingDir , "Sent" , Path.GetFileName(file) ?? ""));
                    } else {
                        _logger.Log((String.Format("Error Transmitting: {0}", file)));
                    }
                } catch (Exception ex) {
                    _logger.Log(String.Format(Constants.TEMPLATE_EXCEPTION,String.Format("Upload Callback: {0}",file), ex));
                }
            }

            ScheduleDataUploader();
        }

        private void ReportTimerCallback(object e) {
            var creds = Utilities.GetCredentials();
            string practiceId = ConfigurationManager.AppSettings[Constants.SETTING_PRACTICE_ID];

            try {
                PracticeConnector.DownloadReports(creds, practiceId, _logger, _incomingDir);
            } catch (Exception ex) {
                _logger.LogException("Report Callback", ex.ToString());
            }

            ScheduleReportTimer();
        }

        private void UpdateTimerCallback(object e) {
            var creds = Utilities.GetCredentials();

            _logger.Log(String.Format("PTMS Client Update Service : {0}", "Ran Service"));

            ScheduleUpdateTimer();
        }


        /// <summary>
        /// Schdules a timer to call a callback function and logs its success or failure.
        /// </summary>
        /// <param name="timer">The global timer to setup</param>
        /// <param name="callBack">The callBack to call when the timer runs</param>
        /// <param name="intervalMinutes">Number of minutes to wait before running the timer callback</param>
        /// <param name="timerName">The display name of the timer for logging purposes</param>
        private void ScheduleTimer(ref Timer timer, TimerCallback callBack, int intervalMinutes, string timerName = "") {
            try {
                // Setup the Timer
                timer = new Timer(callBack);

                // Set the Scheduled Time by adding the Interval to Current Time
                DateTime scheduledTime = DateTime.Now.AddMinutes(intervalMinutes);
                TimeSpan timeSpan = scheduledTime.Subtract(DateTime.Now);

                _logger.Log(String.Format("PTMS Client {0} Service scheduled to run after: {1} day(s) {2} hour(s) {3} minute(s) {4} seconds(s)", timerName, timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));

                int dueTime = Convert.ToInt32(timeSpan.TotalMilliseconds);
                timer.Change(dueTime, Timeout.Infinite);

            } catch (Exception ex) {
                _logger.LogException(String.Format("PTMS Client {0} Service Error", timerName),ex.ToString());

                //Stop the Windows Service.
                using (ServiceController serviceController = new ServiceController(Constants.SERVICE_NAME)) {
                    serviceController.Stop();
                }
            }
        }

    }
}
   