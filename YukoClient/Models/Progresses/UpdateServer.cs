using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;

namespace YukoClient.Models.Progress
{
    public class UpdateServer : Base
    {
        private readonly Server server;

        public UpdateServer(Server server)
        {
            this.server = server;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Получение данных о сервере");
            ServerResponse serverResponse = WebClient.Current.UpdateServer(server.Id);
            if (string.IsNullOrEmpty(serverResponse.ErrorMessage))
            {
                if (!serverResponse.IconUri.Equals(server.IconUri))
                {
                    server.IconUri = serverResponse.IconUri;
                }
                server.Channels = new ObservableCollection<Channel>(serverResponse.Channels);
            }
            else
            {
                dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(serverResponse.ErrorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), serverResponse.ErrorMessage);
            }
        }
    }
}