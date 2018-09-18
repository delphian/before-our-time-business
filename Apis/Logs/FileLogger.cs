using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeforeOurTime.Business.Apis.Logs
{
    public class FileLogger : ILogger
    {
        private string pathToFile = Directory.GetCurrentDirectory() + "/logs.txt";
        private LogLevel LogLevel { set; get; }

        public FileLogger(IConfiguration configuration)
        {
            LogLevel = (LogLevel)Convert.ToInt32(configuration.GetSection("Servers").GetSection("WebSocket")["LogLevel"]);
            if (!File.Exists(pathToFile))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(pathToFile))
                {
                    sw.WriteLine(DateTime.Now.ToString());
                }
            }
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            bool enabled = false;
            switch(logLevel)
            {
                case LogLevel.Error:
                    enabled = true;
                    break;
            }
            return enabled;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel >= LogLevel)
            {
                using (StreamWriter outputFile = File.AppendText(pathToFile))
                {
                    Console.WriteLine($"{DateTime.Now.ToString()} {logLevel.ToString()}: {state.ToString()}");
                    outputFile.WriteLine(logLevel.ToString() + ": " + DateTime.Now.ToString() + ": " + state.ToString());
                }
            }
        }

        /// <summary>
        /// Log simple error message with detailed exception history
        /// </summary>
        /// <param name="message"></param>
        /// <param name="exception"></param>
        public void LogException(string message, Exception exception)
        {
            var traverse = exception;
            while (traverse != null)
            {
                message += ": " + traverse.Message;
                traverse = traverse.InnerException;
            }
            this.LogError(message);
        }

    }
}
