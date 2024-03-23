using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Progresses;
using SUI = Sergey.UI.Extension;

namespace YukoClient.Models.Progress
{
    public class UpdateServer : BaseProgressModel
    {
        private readonly Server _server;

        public UpdateServer(Server server) { _server = server; }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            try
            {
                dispatcher.Invoke(() => State = "Получение данных о сервере");
                ServerResponse serverResponse = WebClient.Current.GetServer(_server.Id);

                if (serverResponse.Error != null)
                    throw new ClientCodeException(serverResponse.Error.Code);

                if (!serverResponse.IconUri.Equals(_server.IconUri))
                    _server.IconUri = serverResponse.IconUri;

                _server.Channels = new ObservableCollection<Channel>(serverResponse.Channels);
            }
            catch (Exception ex)
            {
                if (ex is ClientCodeException clientCodeException &&
                    clientCodeException.ClientErrorCode == ClientErrorCodes.GuildNotFound)
                {
                    dispatcher.Invoke(() =>
                    {
                        if (MessageBox.Show(
                                $"Не удалось обновить сервер \"{_server.Name}\" т.к. он не найден. Удалить его из списка?",
                                App.Name,
                                MessageBoxButton.OK,
                                MessageBoxImage.Error) == MessageBoxResult.Yes)
                        {
                            if (Storage.Current.Servers.Remove(_server))
                                Storage.Current.Save();
                        }
                    });
                }
                else
                {
                    dispatcher.Invoke((Action<string>) ((string errorMessage) =>
                            MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)),
                        ex.Message);
                }
            }
        }
    }
}