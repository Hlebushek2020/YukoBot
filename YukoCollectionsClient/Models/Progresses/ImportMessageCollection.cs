using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace YukoCollectionsClient.Models.Progress
{
    public class ImportMessageCollection : Base
    {
        private readonly ICollection<MessageCollectionItem> messageCollectionItems;
        private readonly string fileName;

        public ImportMessageCollection(ICollection<MessageCollectionItem> messageCollectionItems, string fileName)
        {
            this.messageCollectionItems = messageCollectionItems;
            this.fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Чтение данных");
            string json = File.ReadAllText(fileName, Encoding.UTF8);
            dispatcher.Invoke(() => State = "Обработка");
            List<MessageCollectionItem> items = JsonConvert.DeserializeObject<List<MessageCollectionItem>>(json);
            dispatcher.Invoke(() => State = "Добавление");
            foreach (MessageCollectionItem item in items)
            {
                if (!messageCollectionItems.Contains(item))
                {
                    messageCollectionItems.Add(item);
                }
            }
        }
    }
}