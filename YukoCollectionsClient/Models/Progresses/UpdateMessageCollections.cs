using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Responses;
using SUI = Sergey.UI.Extension;

namespace YukoCollectionsClient.Models.Progress
{
    public class UpdateMessageCollections : Base
    {
        private readonly bool overrideMessageCollections;

        public UpdateMessageCollections(bool overrideMessageCollections)
        {
            this.overrideMessageCollections = overrideMessageCollections;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Получение данных");
            MessageCollectionsResponse response = WebClient.Current.GetMessageCollections();
            dispatcher.Invoke(() => State = "Обработка");
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                if (overrideMessageCollections)
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
                                {
                                    messageCollection.Items.Add(itemResp);
                                }
                            }
                        }
                        else
                        {
                            Storage.Current.MessageCollections.Add(collectionResp);
                        }
                    }
                }
            }
            else
            {
                dispatcher.Invoke(
                    (Action<string>)((string errorMessage) =>
                        SUI.Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK,
                            MessageBoxImage.Error)), response.ErrorMessage);
            }
        }
    }
}