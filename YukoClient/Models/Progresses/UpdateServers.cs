using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Progresses;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient.Models.Progresses
{
    public class UpdateServers : BaseProgressModel
    {
        private readonly bool _overrideServers;

        public UpdateServers(bool overrideServers) { _overrideServers = overrideServers; }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            try
            {
                dispatcher.Invoke(() => State = "Получение данных о серверах");
                ServersResponse response = WebClient.Current.GetServers();

                if (response.Error != null)
                    throw new ClientCodeException(response.Error.Code);

                if (_overrideServers)
                {
                    dispatcher.Invoke(() => State = "Обновление списка серверов");
                    Storage.Current.Servers = new ObservableCollection<Server>(response.Servers);
                }
                else
                {
                    foreach (Server server in response.Servers)
                    {
                        int storeServerIndex = Storage.Current.Servers.IndexOf(server);
                        if (storeServerIndex != -1)
                        {
                            Server storeServer = Storage.Current.Servers[storeServerIndex];
                            dispatcher.Invoke(
                                (Action<string>)((string serverName) =>
                                    State = $"Обновление сервера {serverName}"),
                                storeServer.Name);

                            if (server.IconUri.Equals(storeServer.IconUri))
                                storeServer.IconUri = server.IconUri;

                            foreach (Channel channel in server.Channels)
                            {
                                if (!storeServer.Channels.Contains(channel))
                                    storeServer.Channels.Add(channel);
                            }
                        }
                        else
                        {
                            dispatcher.Invoke((Action<Server>)((Server invokeServer) =>
                            {
                                State = $"Добавление сервера {invokeServer.Name}";
                                Storage.Current.Servers.Add(invokeServer);
                            }), server);
                        }
                    }
                }

                Storage.Current.Save();
            }
            catch (Exception ex)
            {
                dispatcher.Invoke((Action<string>)((string errorMessage) =>
                    MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), ex.Message);
            }
        }
    }
}