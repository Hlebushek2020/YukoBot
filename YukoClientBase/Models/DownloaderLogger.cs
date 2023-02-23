using System;
using System.IO;
using System.Text;

namespace YukoClientBase.Models
{
    public class DownloaderLogger : IDisposable
    {
        private const string FileName = "not_downloaded.txt";
        private const string Separator = "|~|";

        private readonly StreamWriter _streamWriter;

        public DownloaderLogger(string folder)
        {
            _streamWriter = new StreamWriter(Path.Combine(folder, FileName), false, Encoding.UTF8);
            _streamWriter.AutoFlush = true;
        }

        public void Log(string url, Exception ex)
        {
            lock (this)
            {
                _streamWriter.WriteLine($"{url}{Separator}{ex.Message}");
            }
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }
    }
}