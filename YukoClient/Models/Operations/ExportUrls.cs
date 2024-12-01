using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Models.Operations;

namespace YukoClient.Models.Operations
{
    public class ExportUrls : IOperation
    {
        private readonly ICollection<string> _urls;
        private readonly string _fileName;

        public ExportUrls(ICollection<string> urls, string fileName)
        {
            _urls = urls;
            _fileName = fileName;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Подготовка к экспорту ссылок" });
            using (StreamWriter streamWriter = new StreamWriter(_fileName, false, Encoding.UTF8))
            {
                progress.Report(new ProgressReportArgs { Maximum = _urls.Count, Minimum = 0, Value = 0 });

                int counter = 0;

                foreach (string url in _urls)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    counter++;

                    progress.Report(new ProgressReportArgs { Text = $"Запись {counter}/{_urls.Count}" });
                    streamWriter.WriteLine(url);
                    progress.Report(new ProgressReportArgs { Value = counter });
                }
            }

            return Task.CompletedTask;
        }
    }
}