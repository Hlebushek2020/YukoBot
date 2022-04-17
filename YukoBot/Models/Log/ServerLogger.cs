using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace YukoBot.Models.Log
{
    internal class ServerLogger : ILogger
    {
        private static object _fileLock = new object();

        private readonly LogLevel _minimumLogLevel;
        private readonly StreamWriter _file;

        public ServerLogger(string logDirectory, LogLevel minimumLogLevel)
        {
            _minimumLogLevel = minimumLogLevel;
            _file = new StreamWriter(Path.Combine(logDirectory, $"{DateTime.Now.ToString(YukoLoggerFactory.FileNameFormat)}_server.log"), true, Encoding.UTF8) { AutoFlush = true };
        }

        [Obsolete]
        public IDisposable BeginScope<TState>(TState state) { return null; }

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLogLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            Log(logLevel, eventId.Name, exception);
        }

        public void Log(LogLevel logLevel, string eventName, Exception exception = null)
        {
            string log = $"[{DateTime.Now.ToString(YukoLoggerFactory.LogDateTimeFormatter)}] {logLevel.ToString().ToUpper()}; {eventName}";

            if (exception != null)
            {
                log += $"; {exception.GetType().Name}; {exception.Message}";
            }

            lock (_fileLock)
            {
                _file.WriteLine(log);
                if (exception != null)
                    _file.WriteLine(exception.StackTrace);
            }

            if (logLevel == LogLevel.Warning)
                SynchronizedConsole.WriteLine(log, ConsoleColor.Yellow);
            else if (logLevel >= LogLevel.Error)
                SynchronizedConsole.WriteLine(log, ConsoleColor.Red);
            else
                SynchronizedConsole.WriteLine(log);
        }
    }
}