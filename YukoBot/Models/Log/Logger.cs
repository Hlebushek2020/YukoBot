using System;
using System.IO;
using System.Reflection;

namespace YukoBot.Models.Log
{
    internal static class Logger
    {
        private const string FileNameFormat = "yyyyMMdd";

        private static readonly string _logDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");

        private static DateTime _nextLog = DateTime.Now.Date.AddDays(1);
        private static LogWriter _serverLog;
        private static LogWriter _commandLog;

        static Logger()
        {
            Directory.CreateDirectory(_logDirectory);
            string currentDate = DateTime.Now.ToString(FileNameFormat);
            _serverLog = new LogWriter(Path.Combine(_logDirectory, currentDate + "_server.txt"));
            Console.SetOut(_serverLog);
            _commandLog = new LogWriter(Path.Combine(_logDirectory, currentDate + "_command.txt"));
        }

        public static void WriteCommandLog(string value)
        {
            CheckActualLogFile();
            _commandLog.WriteLine(value);
        }

        public static void WriteServerLog(string value)
        {
            CheckActualLogFile();
            _serverLog.WriteLine(value);
        }

        private static void CheckActualLogFile()
        {
            if (_nextLog >= DateTime.Now)
            {
                //lock??
                string currentDate = DateTime.Now.ToString(FileNameFormat);
                _serverLog.Dispose();
                _serverLog = new LogWriter(Path.Combine(_logDirectory, currentDate + "_server.txt"));
                //Console.SetOut(_serverLog);
                _commandLog.Dispose();
                _commandLog = new LogWriter(Path.Combine(_logDirectory, currentDate + "_command.txt"));
                _nextLog = DateTime.Now.Date.AddDays(1);
            }
        }
    }
}