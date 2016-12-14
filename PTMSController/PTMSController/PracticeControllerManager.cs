using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using PTMS.Core;
using PTMS.Core.Logging;
using PTMS.Core.Utilities;

namespace PTMSController {
    public class PracticeControllerManager {
        public String IncomingDirectory { get; private set; }
        public String OutgoingDirectory { get; private set; }
        public String ProcessedDirectory { get; private set; }
        public String DeletedDirectory { get; private set; }

        private readonly Logger _logger;

        public PracticeControllerManager(Logger logger) {
            IncomingDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_INCOMING_DIRECTORY]);
            OutgoingDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_OUTGOING_DIRECTORY]);
            ProcessedDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_PROCESSED_DIRECTORY]);
            DeletedDirectory = FileSystem.BuildAbsolutePath(ConfigurationManager.AppSettings[Constants.SETTING_DELETED_DIRECTORY]);

            FileSystem.AssertDirectoryExists(IncomingDirectory);
            FileSystem.AssertDirectoryExists(OutgoingDirectory);
            FileSystem.AssertDirectoryExists(ProcessedDirectory);
            FileSystem.AssertDirectoryExists(DeletedDirectory);

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
    }
}
