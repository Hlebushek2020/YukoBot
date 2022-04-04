using System;
using System.IO;
using System.Text;

namespace YukoBot.Models.Log
{
    internal class LogWriter : StreamWriter
    {
        public LogWriter(string path) : base(path, true, Encoding.UTF8)
        {
            AutoFlush = true;
        }

        public override void WriteLine(string value) => base.WriteLine($"[{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}] {value}");

    }
}