using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace YukoBot.Models.Log
{
    internal static class Logger
    {
        private const string FileNameFormat = "yyyyMMdd";

        private static readonly string _logDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");

        private static StreamWriter _serverLogWriter;
        private static object _serverLogLocker = new object();
        private static StreamWriter _commandLogWriter;
        private static object _commandLogLocker = new object();

        static Logger()
        {
            Directory.CreateDirectory(_logDirectory);
            string currentDate = DateTime.Now.ToString(FileNameFormat);
            _serverLogWriter = new StreamWriter(Path.Combine(_logDirectory, currentDate + "_server.txt"), true, Encoding.UTF8) { AutoFlush = true };
            _commandLogWriter = new StreamWriter(Path.Combine(_logDirectory, currentDate + "_command.txt"), true, Encoding.UTF8) { AutoFlush = true };
        }

        public static void WriteCommandLog(string value)
        {
            string logLine = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {value}";
            Console.WriteLine(logLine);
            lock (_commandLogLocker)
            {
                _commandLogWriter.WriteLine(logLine);
            }
        }

        public static void WriteServerLog(string value)
        {
            string logLine = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {value}";
            Console.WriteLine(logLine);
            lock (_serverLogLocker)
            {
                _serverLogWriter.WriteLine(logLine);
            }
        }

    }
}