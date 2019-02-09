using System;
namespace CodeLifter.Http.Logging
{
    public interface ILogger
    {
        void LogError(Exception ex, string infoMessage);
        void LogMessage(string infoMessage);
    }
}
