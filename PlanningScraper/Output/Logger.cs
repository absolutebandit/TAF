using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using PlanningScraper.Interfaces;

namespace PlanningScraper.Output
{
    public class Logger : ILogger
    {
        private readonly string _logFileLocation;

        public Logger(IConfiguration configuration)
        {
            _logFileLocation = $"{Environment.CurrentDirectory}\\Logs\\{configuration.LogFileName}";
            Initialise();
        }

        public async Task LogInformationAsync(string message, CancellationToken cancellationToken)
        {
            var outputText = $"{message}{Environment.NewLine}{Environment.NewLine}";
            Console.WriteLine(outputText);
            File.AppendAllText(_logFileLocation, outputText);
            await Task.CompletedTask;
        }

        public async Task LogExceptionAsync(string message, Exception exception, CancellationToken cancellationToken)
        {
            var outputText = $"{message}{Environment.NewLine}{Environment.NewLine}{exception}{Environment.NewLine}{Environment.NewLine}";
            Console.WriteLine(outputText);
            File.AppendAllText(_logFileLocation, outputText);
            await Task.CompletedTask;
        }

        private void Initialise()
        {
            var logDir = $"{Environment.CurrentDirectory}\\Logs\\";

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            DeleteLogFile();
            File.WriteAllText(_logFileLocation, $"Initializing...{Environment.NewLine}{Environment.NewLine}");
        }

        private void DeleteLogFile()
        {
            if (File.Exists(_logFileLocation))
            {
                File.Delete(_logFileLocation);
            }
        }
    }
}
