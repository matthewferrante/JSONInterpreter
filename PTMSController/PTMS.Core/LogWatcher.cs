using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTMS.Core {
    public class LogWatcher : FileSystemWatcher {
        internal string FileName; // The name of the file to monitor

        FileStream Stream;        // The FileStream for reading the text from the file
        StreamReader Reader;      // The StreamReader for reading the text from the FileStream

        //Constructor for the LogWatcher class
        public LogWatcher(string fileName) {
            Changed += OnChanged;     // Subscribe to the Changed event of the base FileSystemWatcher class
            FileName = fileName; // Set the filename of the file to watch

            //Create the FileStream and StreamReader object for the file
            Stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            Reader = new StreamReader(Stream);

            Stream.Position = Stream.Length; //Set the position of the stream to the end of the file
            Path = System.IO.Path.GetDirectoryName(FileName);
        }

        //Occurs when the file is changed
        public void OnChanged(object o, FileSystemEventArgs e) {
            string Contents = Reader.ReadToEnd(); //Read the new text from the file

            LogWatcherEventArgs args = new LogWatcherEventArgs(Contents);  //Fire the TextChanged event
            if (TextChanged != null) TextChanged(this, args);
        }

        public delegate void LogWatcherEventHandler(object sender, LogWatcherEventArgs e);
        public event LogWatcherEventHandler TextChanged; //Event that is fired when the file is changed
    }

    public class LogWatcherEventArgs : EventArgs {
        public string Contents;

        public LogWatcherEventArgs(string contents) { Contents = contents; }
    }
}
