using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;

namespace YukoClient.Models.Progress
{
    public class UpdateServerList : Base
    {
        private readonly bool fullUpdate;

        public UpdateServerList(bool fullUpdate)
        {
            this.fullUpdate = fullUpdate;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Получение данных о серверах");
            ServerListResponse serverList = WebClient.UpdateServerList();
            if (string.IsNullOrEmpty(serverList.ErrorMessage))
            {
                if (fullUpdate)
                {
                    dispatcher.Invoke(() => State = "Обновление списка серверов");
                    Storage.Current.Servers = new ObservableCollection<Server>(serverList.Servers);
                }
                else
                {
                    foreach (Server server in serverList.Servers)
                    {
                        int storeServerIndex = Storage.Current.Servers.IndexOf(server);
                        if (storeServerIndex != -1)
                        {
                            Server storeServer = Storage.Current.Servers[storeServerIndex];
                            dispatcher.Invoke((Action<string>)((string serverName) => State = $"Обновление сервера {serverName}"), storeServer.Name);
                            if (server.IconUri.Equals(storeServer.IconUri))
                            {
                                storeServer.IconUri = server.IconUri;
                            }
                            foreach (Channel channel in server.Channels)
                            {
                                if (!storeServer.Channels.Contains(channel))
                                {
                                    storeServer.Channels.Add(channel);
                                }
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
            else
            {
                dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(serverList.ErrorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), serverList.ErrorMessage);
            }
        }
    }
}