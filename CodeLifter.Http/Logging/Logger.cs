using System;

namespace CodeLifter.Http.Logging
{
    public class Logger : ILogger
    {
        public void LogError(Exception ex, string infoMessage)
        {
            Console.Error.WriteLine($"EXCEPTION:{ex.InnerException.Message} - INFO:{infoMessage}");
        }

        public void LogMessage(string infoMessage)
        {
            Console.WriteLine($"INFO:{infoMessage}");
        }
    }
}
