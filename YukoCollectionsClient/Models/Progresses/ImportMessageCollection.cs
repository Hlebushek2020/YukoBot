using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoCollectionsClient.Models.Progresses
{
    public class ImportMessageCollection : BaseProgressModel
    {
        private readonly ICollection<MessageCollectionItem> _messageCollectionItems;
        private readonly string _fileName;

        public ImportMessageCollection(ICollection<MessageCollectionItem> messageCollectionItems, string fileName)
        {
            _messageCollectionItems = messageCollectionItems;
            _fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Чтение данных");
            string json = File.ReadAllText(_fileName, Encoding.UTF8);
            dispatcher.Invoke(() => State = "Обработка");
            List<MessageCollectionItem> items = JsonConvert.DeserializeObject<List<MessageCollectionItem>>(json);
            dispatcher.Invoke(() => State = "Добавление");
            foreach (MessageCollectionItem item in items)
            {
                if (!_messageCollectionItems.Contains(item))
                {
                    _messageCollectionItems.Add(item);
                }
            }
        }
    }
}