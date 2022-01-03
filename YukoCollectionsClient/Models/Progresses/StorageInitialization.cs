using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Progress
{
    public class StorageInitialization : Base
    {
        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Получение данных");
            ClientDataResponse response = WebClient.Current.GetClientData();
            dispatcher.Invoke(() => State = "Обработка");
            if (string.IsNullOrEmpty(response.ErrorMessage))
            {
                Storage.Current.AvatarUri = response.AvatarUri;
                Storage.Current.Nikname = response.Nikname;
                Storage.Current.Id = response.Id;
                Storage.Current.MessageCollections = new ObservableCollection<MessageCollection>(response.MessageCollections);
            }
            else
            {
                dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), response.ErrorMessage);
            }
        }
    }
}