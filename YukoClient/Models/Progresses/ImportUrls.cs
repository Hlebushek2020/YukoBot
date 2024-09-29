using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoClient.Models.Progresses
{
    public class ImportUrls : BaseProgressModel
    {
        private readonly ICollection<string> _urls;
        private readonly string _fileName;

        public ImportUrls(ICollection<string> urls, string fileName)
        {
            _urls = urls;
            _fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка к импорту сылок");
            using (StreamReader streamReader = new StreamReader(_fileName, Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    string url = streamReader.ReadLine();
                    dispatcher.Invoke((Action<string>)((string invokeUrl) =>
                    {
                        State = $"Добавление {invokeUrl}";
                        _urls.Add(invokeUrl);
                    }), url);
                }
            }
        }
    }
}