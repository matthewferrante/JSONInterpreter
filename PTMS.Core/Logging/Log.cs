using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;

namespace PTMS.Core.Logging {
    public class Logger {
        public string NameSpace { get; set; }

        private string _logFile;
        private static string LOG_FILE_NAME = ConfigurationManager.AppSettings[Constants.APP_SETTING_LOGFILE];

        public Logger() {
            string rawName = Assembly.GetExecutingAssembly().Location;
            string dirName = Path.GetDirectoryName(rawName);

            _logFile = dirName + LOG_FILE_NAME;
            NameSpace = "";
        }

        public void Log(string logMessage) {
            //TODO: Make this threadsafe
            using (StreamWriter w = new StreamWriter(_logFile, true)) {
                w.WriteLine("{0} [{1}]: {2}", DateTime.Now, NameSpace, logMessage);
            }
        }
        public string ReadTail() {
            using (FileStream fs = File.Open(_logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                fs.Seek(-1024, SeekOrigin.End); // Seek 1024 bytes from the end of the file

                byte[] bytes = new byte[1024]; // read 1024 bytes
                fs.Read(bytes, 0, 1024);

                return Encoding.Default.GetString(bytes); // Convert bytes to string
            }
        }

        public void LogException(string location, string exception) {
            Log(String.Format(Constants.TEMPLATE_EXCEPTION, location, exception));
        }
    }
}

