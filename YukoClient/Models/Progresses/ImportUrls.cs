using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;

namespace YukoClient.Models.Progresses
{
    public class ImportUrls
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly ICollection<string> _urls;
        private readonly string _fileName;

        public ImportUrls(ICollection<string> urls, string fileName)
        {
            _urls = urls;
            _fileName = fileName;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Подготовка к импорту ссылок" });
            using (StreamReader streamReader = new StreamReader(_fileName, Encoding.UTF8))
            {
                while (!streamReader.EndOfStream)
                {
                    string url = streamReader.ReadLine();
                    progress.Report(new ProgressReportArgs { Text = $"Добавление {url}" });
                    _synchronizationContext.Send(state => _urls.Add(url), null);
                }
            }

            return Task.CompletedTask;
        }
    }
}