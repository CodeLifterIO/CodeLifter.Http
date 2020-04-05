using System;
using RestSharp;

namespace CodeLifter.Http.Logging
{
    public interface ILogger
    {
        bool IncludeDebug { get; }
        bool IncludeConsole { get; }

        void SetIsDebugIncluded(bool isDebug);

        void LogMessage(string infoMessage);
        void LogMessage(string title, string message);

        void LogException(Exception ex);
        void LogError(string infoMessage);
        void LogError(string title, string infoMessage);
        void LogError(Uri BaseUrl, HttpRequest request, IRestResponse response);
    }
}
