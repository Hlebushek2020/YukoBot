using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoClient.Models.Progress
{
    public class ImportUrls : BaseProgressModel
    {
        private readonly ICollection<string> urls;
        private readonly string fileName;

        public ImportUrls(ICollection<string> urls, string fileName)
        {
            this.urls = urls;
            this.fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка к импорту сылок");
            using (StreamReader streamReader = new StreamReader(fileName, Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    string url = streamReader.ReadLine();
                    dispatcher.Invoke((Action<string>)((string invokeUrl) =>
                    {
                        State = $"Добавление {invokeUrl}";
                        urls.Add(invokeUrl);
                    }), url);
                }
            }
        }
    }
}