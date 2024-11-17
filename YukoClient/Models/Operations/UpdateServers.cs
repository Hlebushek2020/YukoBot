using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Args;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Operations;

namespace YukoClient.Models.Operations
{
    public class UpdateServers : IOperation
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly bool _overrideServers;

        public UpdateServers(bool overrideServers)
        {
            _overrideServers = overrideServers;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Получение данных о серверах" });

            ServersResponse response = WebClient.Current.GetServers();

            if (response.Error != null)
                throw new ClientCodeException(response.Error.Code);

            if (_overrideServers)
            {
                progress.Report(new ProgressReportArgs { Text = "Обновление списка серверов" });

                cancellationToken.ThrowIfCancellationRequested();

                Storage.Current.Servers = new ObservableCollection<Server>(response.Servers);
            }
            else
            {
                foreach (Server server in response.Servers)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    int storeServerIndex = Storage.Current.Servers.IndexOf(server);
                    if (storeServerIndex != -1)
                    {
                        Server storeServer = Storage.Current.Servers[storeServerIndex];

                        progress.Report(new ProgressReportArgs { Text = $"Обновление сервера {storeServer.Name}" });

                        if (server.IconUri.Equals(storeServer.IconUri))
                            storeServer.IconUri = server.IconUri;

                        foreach (Channel channel in server.Channels)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (!storeServer.Channels.Contains(channel))
                                storeServer.Channels.Add(channel);
                        }
                    }
                    else
                    {
                        progress.Report(new ProgressReportArgs { Text = $"Добавление сервера {server.Name}" });

                        _synchronizationContext.Send(state => Storage.Current.Servers.Add(server), null);
                    }
                }
            }

            Storage.Current.Save();

            return Task.CompletedTask;
        }
    }
}