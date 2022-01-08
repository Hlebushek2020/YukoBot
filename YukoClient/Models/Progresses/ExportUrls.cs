using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace YukoClient.Models.Progress
{
    public class ExportUrls : Base
    {
        private readonly ICollection<string> urls;
        private readonly string fileName;

        public ExportUrls(ICollection<string> urls, string fileName)
        {
            this.urls = urls;
            this.fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Подготовка к экспорту сылок");
            using (StreamWriter streamWriter = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                dispatcher.Invoke(() => MaxValue = urls.Count);
                foreach (string url in urls)
                {
                    dispatcher.Invoke(() => State = $"Запись {Value + 1}/{MaxValue}");
                    streamWriter.WriteLine(url);
                    dispatcher.Invoke(() => Value++);
                }
            }
        }
    }
}