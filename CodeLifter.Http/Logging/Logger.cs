using System;
using System.Diagnostics;

namespace CodeLifter.Http.Logging
{
    public class Logger : ILogger
    {
        public Logger()
        {
            IncludeDebug = true;
            IncludeConsole = true;
        }

        public Logger(bool includeDebug, bool includeConsole)
        {
            IncludeDebug = includeDebug;
            IncludeConsole = includeConsole;
        }

        public bool IncludeDebug
        {
            get;
            private set;
        }

        public bool IncludeConsole
        {
            get;
            private set;
        }

        public void SetIsDebugIncluded(bool isDebug)
        {
            IncludeDebug = isDebug;
        }

        public void LogMessage(string infoMessage)
        {
            PrintToAllEnabledLogs($"LOG - TRACE: - INFO:{infoMessage}");
        }

        public void LogMessage(string title, string infoMessage)
        {
            PrintToAllEnabledLogs($"LOG - {title.ToUpper()}: - {infoMessage}");
        }

        public void LogError(string infoMessage)
        {
            LogError(null, infoMessage);
        }

        public void LogError(string title, string infoMessage)
        {
            if (string.IsNullOrWhiteSpace(title)) title = "LOG - ERROR";
            PrintToAllEnabledLogs($"{title}: - INFO:{infoMessage}");
        }

        public void LogException(Exception ex)
        {
            LogError("LOG - EXCEPTION", ex.InnerException.Message);;
        }

        private void PrintToAllEnabledLogs(string output)
        {
            if (IncludeConsole) Console.WriteLine(output);
            if (IncludeDebug) Debug.WriteLine(output);
        }


    }
}
