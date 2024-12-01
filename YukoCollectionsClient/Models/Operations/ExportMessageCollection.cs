using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;

namespace YukoCollectionsClient.Models.Operations
{
    public class ExportMessageCollection
    {
        private readonly ICollection<MessageCollectionItem> _messageCollectionItems;
        private readonly string _fileName;

        public ExportMessageCollection(ICollection<MessageCollectionItem> messageCollectionItems, string fileName)
        {
            _messageCollectionItems = messageCollectionItems;
            _fileName = fileName;
        }

        public async Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { IsIndeterminate = true, Text = "Подготовка" });
            using (StreamWriter streamWriter = new StreamWriter(_fileName, false, Encoding.UTF8))
            {
                progress.Report(new ProgressReportArgs { Text = "Обработка" });
                string json = JsonConvert.SerializeObject(_messageCollectionItems);
                progress.Report(new ProgressReportArgs { Text = "Запись" });
                await streamWriter.WriteAsync(json);
            }
        }
    }
}