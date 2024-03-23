using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Progresses;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Responses;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoCollectionsClient.Models.Progress
{
    public class UpdateMessageCollections : BaseProgressModel
    {
        private readonly bool _overrideMessageCollections;

        public UpdateMessageCollections(bool overrideMessageCollections)
        {
            _overrideMessageCollections = overrideMessageCollections;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            try
            {
                dispatcher.Invoke(() => State = "Получение данных");
                MessageCollectionsResponse response = WebClient.Current.GetMessageCollections();

                if (response.Error != null)
                    throw new ClientCodeException(response.Error.Code);

                dispatcher.Invoke(() => State = "Обработка");
                if (_overrideMessageCollections)
                {
                    Storage.Current.MessageCollections =
                        new ObservableCollection<MessageCollection>(response.MessageCollections);
                }
                else
                {
                    foreach (MessageCollection collectionResp in response.MessageCollections)
                    {
                        if (Storage.Current.MessageCollections.Contains(collectionResp))
                        {
                            int index = Storage.Current.MessageCollections.IndexOf(collectionResp);
                            MessageCollection messageCollection = Storage.Current.MessageCollections[index];
                            foreach (MessageCollectionItem itemResp in collectionResp.Items)
                            {
                                if (!messageCollection.Items.Contains(itemResp))
                                    messageCollection.Items.Add(itemResp);
                            }
                        }
                        else
                        {
                            Storage.Current.MessageCollections.Add(collectionResp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                dispatcher.Invoke((Action<string>) ((string errorMessage) =>
                    MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), ex.Message);
            }
        }
    }
}