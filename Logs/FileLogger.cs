using BeforeOurTime.Models.Logs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeforeOurTime.Business.Logs
{
    public class FileLogger : IBotLogger
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
                Console.ForegroundColor = (logLevel == LogLevel.Debug) ? ConsoleColor.DarkGray : Console.ForegroundColor;
                Console.ForegroundColor = (logLevel >= LogLevel.Error) ? ConsoleColor.Red : Console.ForegroundColor;
                Console.WriteLine($"{DateTime.Now.ToString()} {logLevel.ToString()}: {state.ToString()}");
                Console.ResetColor();
                using (StreamWriter outputFile = File.AppendText(pathToFile))
                {
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
            message += ":: " + exception.StackTrace;
            this.LogError(message);
        }
    }
}
