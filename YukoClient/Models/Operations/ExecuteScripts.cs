using System;
using System.Threading;
using System.Threading.Tasks;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Errors;
using YukoClient.Models.Web.Providers;
using YukoClientBase.Args;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Extensions;
using YukoClientBase.Models.Operations;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Operations
{
    public class ExecuteScripts : IOperation
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly Server _server;

        public ExecuteScripts(Server server)
        {
            _server = server;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Подключение" });
            using (ExecuteScriptProvider provider = WebClient.Current.ExecuteScripts(
                       _server.Id, _server.Scripts.Count, out Response<ExecuteScriptErrorJson> response))
            {
                if (response.Error != null)
                {
                    if (response.Error.Code == ClientErrorCodes.MemberBanned)
                        throw new ClientCodeException(ClientErrorCodes.MemberBanned, response.Error.Reason);

                    throw new ClientCodeException(response.Error.Code);
                }

                foreach (Script script in _server.Scripts)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progress.Report(new ProgressReportArgs
                    {
                        Text = $"Выполнение правила (Канал: {script.Channel.Id}; тип запроса: {script.Mode.Title})"
                    });

                    _synchronizationContext.Send(state =>
                    {
                        script.Errors.Clear();
                        script.CompletedWithErrors = false;
                    }, null);

                    provider.ExecuteScript(script);
                    int blockCounter = 1;
                    UrlsResponse urlsResponse = null;

                    do
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        progress.Report(new ProgressReportArgs { Text = $"Получение данных (Блок: {blockCounter})" });

                        blockCounter++;
                        urlsResponse = provider.ReadBlock();

                        foreach (string url in urlsResponse.Urls)
                            _synchronizationContext.Send(state => _server.Urls.Add(url), null);

                        if (urlsResponse.Error == null)
                            continue;

                        string errorText = urlsResponse.Error.Code.GetText(
                            urlsResponse.Error.Code == ClientErrorCodes.ChannelNotFound
                                ? urlsResponse.ChannelId
                                : urlsResponse.MessageId);

                        _synchronizationContext.Send(state =>
                        {
                            script.Errors.Add(errorText);
                            script.CompletedWithErrors = true;
                        }, null);
                    } while (urlsResponse.Next);
                }
            }

            return Task.CompletedTask;
        }
    }
}