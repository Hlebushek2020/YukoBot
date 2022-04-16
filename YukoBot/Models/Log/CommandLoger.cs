using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace YukoBot.Models.Log
{
    internal class CommandLoger : ILogger
    {
        private static object _fileLocker = new object();

        private readonly StreamWriter _file;

        public CommandLoger(string logDirectory)
        {
            _file = new StreamWriter(Path.Combine(logDirectory, $"{DateTime.Now.ToString(YukoLoggerFactory.FileNameFormat)}_command.log"), true, Encoding.UTF8) { AutoFlush = true };
        }

        [Obsolete]
        public IDisposable BeginScope<TState>(TState state) { return null; }

        [Obsolete]
        public bool IsEnabled(LogLevel logLevel) { return true; }

        [Obsolete]
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) { }

        public void Log<TState>(DiscordUser dUser, TState state, Exception ex, string command, bool printStackTrace = false)
        {
            string log = $"[{DateTime.Now.ToString(YukoLoggerFactory.LogDateTimeFormatter)}] {dUser.Username}#{dUser.Discriminator}; {dUser.Id}; {state}";
            if (ex != null)
            {
                log += $"; {ex.GetType().Name}";
            }
            log += $"; {command}";
            lock (_fileLocker)
            {
                _file.WriteLine(log);
                Console.WriteLine(log);
                if (ex != null && printStackTrace)
                {
                    _file.WriteLine(ex.StackTrace);
                    Console.WriteLine(log);
                }
            }
        }
    }
}