using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Models.Operations;

namespace YukoCollectionsClient.Models.Operations
{
    public class ImportMessageCollection : IOperation
    {
        private readonly ICollection<MessageCollectionItem> _messageCollectionItems;
        private readonly string _fileName;

        public ImportMessageCollection(ICollection<MessageCollectionItem> messageCollectionItems, string fileName)
        {
            _messageCollectionItems = messageCollectionItems;
            _fileName = fileName;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { IsIndeterminate = true, Text = "Чтение данных" });
            string json = File.ReadAllText(_fileName, Encoding.UTF8);
            progress.Report(new ProgressReportArgs { Text = "Чтение Обработка" });
            List<MessageCollectionItem> items = JsonConvert.DeserializeObject<List<MessageCollectionItem>>(json);
            progress.Report(new ProgressReportArgs { Text = "Добавление" });
            foreach (MessageCollectionItem item in items)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!_messageCollectionItems.Contains(item))
                    _messageCollectionItems.Add(item);
            }

            return Task.CompletedTask;
        }
    }
}