using System;
using System.Diagnostics;
using System.IO;

namespace Rev76.Core.Logging
{

    public class CustomTextWriterTraceListener : TextWriterTraceListener
    {
        private readonly TraceEventType _MinLogLevel;
        private readonly TraceEventType _DefaultWriteLineLevel;

        private readonly long _MaxFileSize = 2_000_000;
        private readonly int _MaxLogFiles = 10;
        private string _FilePath;

        public CustomTextWriterTraceListener(string filePath, TraceEventType minLogLevel = TraceEventType.Verbose, TraceEventType defaultWriteLineLevel = TraceEventType.Information)
             : base(new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        {
            _FilePath = filePath;
            _MinLogLevel = minLogLevel;
            _DefaultWriteLineLevel = defaultWriteLineLevel;
        }

        public override void WriteLine(string message)
        {
            WriteLog(message, _DefaultWriteLineLevel);
        }

        public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
        {
            if (eventType >= _MinLogLevel)
            {
                WriteLog(message, eventType);
            }
        }


        private void WriteLog(string message, TraceEventType eventType)
        {
            if (eventType >= _MinLogLevel)
            {
                RollLogFileIfNeeded();

                string formattedMessage = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} [{eventType}] [Thread {Environment.CurrentManagedThreadId}] - {message}";
               
                base.WriteLine(formattedMessage);

                if (_MinLogLevel == TraceEventType.Critical)
                {
                    base.WriteLine($"{Environment.StackTrace}");
                }

                base.Flush();
            }
        }

        private void RollLogFileIfNeeded()
        {
            FileInfo logFile = new FileInfo(_FilePath);
            if (logFile.Exists && logFile.Length > _MaxFileSize)
            {
                // Close the existing writer to release the file
                base.Writer.Flush();
                base.Writer.Close();

                // Rotate old log files
                for (int i = _MaxLogFiles - 1; i >= 0; i--)
                {
                    string oldFile = $"{_FilePath.Replace(".log", $"_{i}.log")}";
                    string newFile = $"{_FilePath.Replace(".log", $"_{i + 1}.log")}";

                    if (File.Exists(oldFile))
                    {
                        if (i == _MaxLogFiles - 1)
                            File.Delete(oldFile); // Remove oldest file
                        else
                            File.Move(oldFile, newFile);
                    }
                }

                
                File.Move(_FilePath, $"{_FilePath.Replace(".log", "_0.log")}");

                base.Writer = new StreamWriter(new FileStream(_FilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite));
            }
        }
    }


}
