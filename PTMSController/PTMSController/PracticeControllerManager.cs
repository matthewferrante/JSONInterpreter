using System;
using System.Configuration;
using System.IO;
using System.Windows;
using PTMS.Core;
using PTMS.Core.Api;
using PTMS.Core.Configuration;
using PTMS.Core.Logging;
using PTMS.Core.Utilities;

namespace PTMSController {
    public class PracticeControllerManager {
        public String IncomingDirectory { get; private set; }
        public String OutgoingDirectory { get; private set; }
        public String ProcessedDirectory { get; private set; }
        public String DeletedDirectory { get; private set; }
        public ApiCredentials ApiCredentials { get; private set; }
        public Logger Logger { get { return _logger; } }

        private readonly Logger _logger;


        public PracticeControllerManager(Logger logger) {
            VerifyAndLoadConfiguration();

            try {
                FileSystem.AssertDirectoryExists(IncomingDirectory);
                FileSystem.AssertDirectoryExists(OutgoingDirectory);
                FileSystem.AssertDirectoryExists(ProcessedDirectory);
                FileSystem.AssertDirectoryExists(DeletedDirectory);
            } catch (Exception ex) {
                throw new Exception("Error creating directories for controller.  PCM main constructor.");
            }

            _logger = logger;
        }

        public static PracticeControllerManager Current {
            get { return (PracticeControllerManager)Application.Current.Properties[Constants.CONTROLLER_MANAGER]; }
        }

        public void DeleteIncomingQuestionnaire(string fileName) {
            _logger.Log(String.Format("Removing {0}", fileName));

            try {
                var from = Path.Combine(IncomingDirectory, fileName);
                var to = Path.Combine(DeletedDirectory, fileName);

                File.Move(from, to);
            } catch (Exception ex) {
                _logger.LogException("Removing Incoming Questionnaire", ex.ToString());
            }
        }

        private bool VerifyAndLoadConfiguration() {
            try {
                IncomingDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_INCOMING_DIRECTORY]);
                OutgoingDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_OUTGOING_DIRECTORY]);
                ProcessedDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_PROCESSED_DIRECTORY]);
                DeletedDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_DELETED_DIRECTORY]);

                ApiCredentials = Utilities.GetCredentials();
            } catch (Exception ex) {
                throw new ConfigurationErrorsException(String.Format("There is an error in the configuration file.  Please check the file and try again.\nReason: {0}", ex));
            }


            return true;
        }
    }
}
