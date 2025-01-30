using System;
using System.Diagnostics;
using System.IO;

namespace Rev76.Core.Logging
{
    public static class Tracing
    {
        public static void StartTracing(string filename = "rev76.log", TraceEventType minLogLevel = TraceEventType.Critical)
        {
            filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filename);
            
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new CustomTextWriterTraceListener(filename, minLogLevel));
            Trace.Listeners[0].TraceOutputOptions = TraceOptions.ThreadId | TraceOptions.DateTime;
            Trace.AutoFlush = true;

        }       
    }
}
