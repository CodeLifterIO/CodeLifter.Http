using System;
using System.Diagnostics;
using System.Linq;
using RestSharp;

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

        public void LogError(Uri BaseUrl, HttpRequest request, IRestResponse response)
        {
            //Get the values of the parameters passed to the API
            string parameters = string.Join(", ", request.Parameters.Select(x => x.Name.ToString() + "=" + ((x.Value == null) ? "NULL" : x.Value)).ToArray());

            //Set up the information message with the URL, the status code, and the parameters.
            string info = "Request to " + BaseUrl.AbsoluteUri + request.Resource + " failed with status code " + response.StatusCode + ", parameters: "
            + parameters + ", and content: " + response.Content;

            //Acquire the actual exception
            Exception ex;
            if (response != null && response.ErrorException != null)
            {
                ex = response.ErrorException;
            }
            else
            {
                ex = new Exception(info);
                info = string.Empty;
            }

            //Log the exception and info message
            LogError(info);
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
