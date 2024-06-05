using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoCollectionsClient.Models.Progresses
{
    public class ExportUrls : BaseProgressModel
    {
        private readonly ICollection<string> _urls;
        private readonly string _fileName;

        public ExportUrls(ICollection<string> urls, string fileName)
        {
            _urls = urls;
            _fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка к экспорту сылок");
            using (StreamWriter streamWriter = new StreamWriter(_fileName, false, Encoding.UTF8))
            {
                dispatcher.Invoke(() => MaxValue = _urls.Count);
                foreach (string url in _urls)
                {
                    dispatcher.Invoke(() => State = $"Запись {Value + 1}/{MaxValue}");
                    streamWriter.WriteLine(url);
                    dispatcher.Invoke(() => Value++);
                }
            }
        }
    }
}