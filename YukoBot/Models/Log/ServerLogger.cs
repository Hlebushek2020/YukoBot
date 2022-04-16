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

            lock (_fileLock)
            {
                var ename = eventId.Name;
                ename = ename?.Length > 12 ? ename?.Substring(0, 12) : ename;
                Console.Write($"[{DateTimeOffset.Now.ToString(YukoLoggerFactory.LogDateTimeFormatter)}] [{eventId.Id,-4}/{ename,-12}] ");

                switch (logLevel)
                {
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;

                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;

                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case LogLevel.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }
                Console.Write(logLevel switch
                {
                    LogLevel.Trace => "[Trace] ",
                    LogLevel.Debug => "[Debug] ",
                    LogLevel.Information => "[Info ] ",
                    LogLevel.Warning => "[Warn ] ",
                    LogLevel.Error => "[Error] ",
                    LogLevel.Critical => "[Crit ]",
                    LogLevel.None => "[None ] ",
                    _ => "[?????] "
                });
                Console.ResetColor();

                //The foreground color is off.
                if (logLevel == LogLevel.Critical)
                    Console.Write(" ");

                var message = formatter(state, exception);
                Console.WriteLine(message);
                if (exception != null)
                    Console.WriteLine(exception);
            }
        }

        public void Log(string message)
        {
        }
    }
}