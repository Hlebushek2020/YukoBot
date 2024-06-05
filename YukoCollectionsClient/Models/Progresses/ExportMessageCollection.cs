using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoCollectionsClient.Models.Progresses
{
    public class ExportMessageCollection : BaseProgressModel
    {
        private readonly ICollection<MessageCollectionItem> _messageCollectionItems;
        private readonly string _fileName;

        public ExportMessageCollection(ICollection<MessageCollectionItem> messageCollectionItems, string fileName)
        {
            _messageCollectionItems = messageCollectionItems;
            _fileName = fileName;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка");
            using (StreamWriter streamWriter = new StreamWriter(_fileName, false, Encoding.UTF8))
            {
                dispatcher.Invoke(() => State = "Обработка");
                string json = JsonConvert.SerializeObject(_messageCollectionItems);
                dispatcher.Invoke(() => State = "Запись");
                streamWriter.Write(json);
            }
        }
    }
}