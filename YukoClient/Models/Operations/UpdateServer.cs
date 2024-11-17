using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Args;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Operations;

namespace YukoClient.Models.Operations
{
    public class UpdateServer : IOperation
    {
        private readonly Server _server;

        public UpdateServer(Server server) { _server = server; }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Получение данных о сервере" });

            ServerResponse serverResponse = WebClient.Current.GetServer(_server.Id);

            if (serverResponse.Error != null && serverResponse.Error.Code == ClientErrorCodes.GuildNotFound)
                throw new Exception(
                    $"Не удалось обновить сервер \"{_server.Name}\" т.к. он не найден. Удалить его из списка?");

            if (serverResponse.Error != null && serverResponse.Error.Code != ClientErrorCodes.GuildNotFound)
                throw new ClientCodeException(serverResponse.Error.Code);

            if (!serverResponse.IconUri.Equals(_server.IconUri))
                _server.IconUri = serverResponse.IconUri;

            _server.Channels = new ObservableCollection<Channel>(serverResponse.Channels);

            return Task.CompletedTask;
        }
    }
}