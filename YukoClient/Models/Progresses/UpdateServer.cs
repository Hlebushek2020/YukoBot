using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Models.Progresses;
using SUI = Sergey.UI.Extension;

namespace YukoClient.Models.Progress
{
    public class UpdateServer : BaseProgressModel
    {
        private readonly Server server;

        public UpdateServer(Server server)
        {
            this.server = server;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Получение данных о сервере");
            ServerResponse serverResponse = WebClient.Current.GetServer(server.Id);
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
                dispatcher.Invoke((Action<string>)((string errorMessage) => SUI.Dialogs.MessageBox.Show(serverResponse.ErrorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), serverResponse.ErrorMessage);
            }
        }
    }
}