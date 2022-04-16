﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace YukoBot.Models.Log
{
    internal class YukoLoggerFactory : ILoggerFactory
    {
        public const string FileNameFormat = "yyyyMMdd";
        public const string LogDateTimeFormatter = "dd.MM.yyyy hh:mm:ss";

        private Dictionary<string, ILogger> _loggers = new Dictionary<string, ILogger>();

        public YukoLoggerFactory(LogLevel discordLogLevel)
        {
            string logDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");
            _loggers.Add(typeof(ServerLogger).Name, new ServerLogger(logDirectory, discordLogLevel));
            _loggers.Add(typeof(CommandLoger).Name, new CommandLoger(logDirectory));
        }

        [Obsolete]
        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName)
        {
            if (_loggers.ContainsKey(categoryName))
                return _loggers[categoryName];

            return _loggers[typeof(ServerLogger).Name];
        }

        public T CreateLogger<T>() => (T)_loggers[typeof(T).Name];

        [Obsolete]
        public void Dispose() { }
    }
}