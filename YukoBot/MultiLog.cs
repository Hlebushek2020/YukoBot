using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace YukoBot
{
    public class MultiLog : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        public bool IsDisposed { get; private set; } = false;

        private const string FileNameFormat = "yyyyMMdd";

        private readonly List<TextWriter> writers = new List<TextWriter>();
        private readonly string logDirectory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "logs");

        private DateTime nextLog = DateTime.Now.Date.AddDays(1);

        public MultiLog(TextWriter consoleWritter)
        {
            writers.Add(consoleWritter);
            Directory.CreateDirectory(logDirectory);
            writers.Add(new StreamWriter(Path.Combine(logDirectory, DateTime.Now.ToString(FileNameFormat)) + ".txt", true, Encoding.UTF8) { AutoFlush = true });
        }

        public override void Write(string value)
        {
            value = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {value}";
            CheckActualLogFile();
            writers.ForEach(x => x.Write(value));
        }

        public override void WriteLine(string value)
        {
            value = $"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {value}";
            CheckActualLogFile();
            writers.ForEach(x => x.WriteLine(value));
        }

        private void CheckActualLogFile()
        {
            if (nextLog >= DateTime.Now)
            {
                writers[1].Dispose();
                writers[1] = new StreamWriter(Path.Combine(logDirectory, DateTime.Now.ToString(FileNameFormat)) + ".txt", true, Encoding.UTF8) { AutoFlush = true };
                nextLog = nextLog.AddDays(1);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                    writers.ForEach(x => x.Dispose());

                IsDisposed = true;
            }
        }
    }
}
