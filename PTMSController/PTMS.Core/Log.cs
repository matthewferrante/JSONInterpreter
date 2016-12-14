using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;

namespace PTMS.Core {
    public class Logger {
        public string NameSpace { get; set; }

        private string LogFile;
        private static string LOG_FILE_NAME = ConfigurationManager.AppSettings[Constants.APP_SETTING_LOGFILE];

        public Logger() {
            string rawName = Assembly.GetExecutingAssembly().Location;
            string dirName = Path.GetDirectoryName(rawName);

            LogFile = dirName + "\\" + LOG_FILE_NAME;
            NameSpace = "";
        }

        public void Log(string logMessage) {
            using (StreamWriter w = new StreamWriter(LogFile, true)) {
                w.WriteLine("{0} [{1}]: {2}", DateTime.Now, NameSpace, logMessage);
            }
        }
        public string ReadTail() {
            using (FileStream fs = File.Open(LogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                fs.Seek(-1024, SeekOrigin.End); // Seek 1024 bytes from the end of the file

                byte[] bytes = new byte[1024]; // read 1024 bytes
                fs.Read(bytes, 0, 1024);

                return Encoding.Default.GetString(bytes); // Convert bytes to string
            }
        }

    }
}

