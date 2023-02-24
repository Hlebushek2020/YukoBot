using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoCollectionsClient.Models.Progress
{
    public class ExportMessageCollection : BaseProgressModel
    {
        private readonly ICollection<MessageCollectionItem> messageCollectionItems;
        private readonly string fileName;

        public ExportMessageCollection(ICollection<MessageCollectionItem> messageCollectionItems, string fileName)
        {
            this.messageCollectionItems = messageCollectionItems;
            this.fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка");
            using (StreamWriter streamWriter = new StreamWriter(fileName, false, Encoding.UTF8))
            {
                dispatcher.Invoke(() => State = "Обработка");
                string json = JsonConvert.SerializeObject(messageCollectionItems);
                dispatcher.Invoke(() => State = "Запись");
                streamWriter.Write(json);
            }
        }
    }
}