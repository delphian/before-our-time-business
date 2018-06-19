using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeforeOurTime.Business.Apis.Logs
{
    class FileLogger : ILogger
    {
        private string pathToFile = Directory.GetCurrentDirectory() + "/logs.txt";

        public FileLogger()
        {
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
            using (StreamWriter outputFile = File.AppendText(pathToFile))
            {
                Console.WriteLine($"{DateTime.Now.ToString()} {logLevel.ToString()}: {state.ToString()}");
                outputFile.WriteLine(logLevel.ToString() + ": " + DateTime.Now.ToString() + ": " + state.ToString());
            }
        }
    }
}
