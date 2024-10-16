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
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Progresses
{
    public class ExecuteScripts
    {
        private readonly Server _server;
        private readonly SynchronizationContext _synchronizationContext;

        public ExecuteScripts(Server server)
        {
            _server = server;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public async Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { IsIndeterminate = true, Text = "Подключение" });
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
                    dispatcher.Invoke(
                        (Action<ulong, string>)((ulong channelId, string mode) =>
                        {
                            State = $"Выполнение правила (Канал: {channelId}; тип запроса: {mode})";
                            script.Errors.Clear();
                            script.CompletedWithErrors = false;
                        }), script.Channel.Id, script.Mode.Title);
                    provider.ExecuteScript(script);
                    int blockCounter = 1;
                    UrlsResponse urlsResponse = null;
                    do
                    {
                        dispatcher.Invoke((Action<int>)((int block) =>
                            State = $"Получение данных (Блок: {block})"), blockCounter);
                        blockCounter++;
                        urlsResponse = provider.ReadBlock();
                        foreach (string url in urlsResponse.Urls)
                            dispatcher.Invoke((Action<string>)((string iUrl) =>
                                _server.Urls.Add(iUrl)), url);
                        if (urlsResponse.Error != null)
                        {
                            string errorText = urlsResponse.Error.Code.GetText(
                                urlsResponse.Error.Code == ClientErrorCodes.ChannelNotFound
                                    ? urlsResponse.ChannelId
                                    : urlsResponse.MessageId);
                            dispatcher.Invoke(() =>
                            {
                                script.Errors.Add(errorText);
                                script.CompletedWithErrors = true;
                            });
                        }
                    } while (urlsResponse.Next);
                }
            }
        }
    }
}