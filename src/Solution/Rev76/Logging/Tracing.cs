using System;
using System.Diagnostics;
using System.IO;

namespace Rev76.Logging
{
    public class Tracing
    {
        public void StartTracing(string filename = "revamp.log", int verbosity = 0, bool writeErrors = false)
        {
            Verbosity = verbosity;
            WriteErrors  = writeErrors;

            Trace.Listeners.Clear();
            var rootPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            FileStream logFileStream = new FileStream(filename, FileMode.Append, FileAccess.Write, FileShare.Read);

            TextWriterTraceListener twtl = new TextWriterTraceListener(logFileStream);
            twtl.Name = filename;

            twtl.TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;
            Trace.Write(Path.GetTempPath());
            Trace.Listeners.Add(twtl);

            Trace.AutoFlush = true;

        }

        public int Verbosity { get; set; } = 3;
        public bool WriteErrors { get; set; } = false;

        public void WriteLine(string message, int level=0)
        {
            if (level >= Verbosity)
            {
                Trace.WriteLine(message);
            }
        }

        public void WriteError(Exception ex)
        {
            if (WriteErrors)
            {
                Trace.WriteLine(ex.Message);
            }
        }
    }
}
