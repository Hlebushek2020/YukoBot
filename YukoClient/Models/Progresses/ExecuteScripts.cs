using System;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Providers;
using YukoClientBase.Models.Progresses;
using YukoClientBase.Models.Web.Responses;
using SUI = Sergey.UI.Extension;

namespace YukoClient.Models.Progress
{
    public class ExecuteScripts : BaseProgressModel
    {
        private readonly Server _server;

        public ExecuteScripts(Server server) { _server = server; }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подключение");
            using (ExecuteScriptProvider provider = WebClient.Current.ExecuteScripts(_server.Id, _server.Scripts.Count))
            {
                dispatcher.Invoke(() => State = "Аутентификация");
                UrlsResponse response = provider.ReadBlock();
                if (string.IsNullOrEmpty(response.ErrorMessage))
                {
                    StringBuilder errorMessages = new StringBuilder();
                    foreach (Script script in _server.Scripts)
                    {
                        dispatcher.Invoke(
                            (Action<ulong, string>) ((ulong channelId, string mode) =>
                                State = $"Выполнение правила (Канал: {channelId}; тип запроса: {mode})"),
                            script.Channel.Id, script.Mode.Title);
                        provider.ExecuteScript(script);
                        int blockCounter = 1;
                        do
                        {
                            dispatcher.Invoke(
                                (Action<int>) ((int _block) => State = $"Получение данных (Блок: {_block})"),
                                blockCounter);
                            blockCounter++;
                            response = provider.ReadBlock();
                            if (string.IsNullOrEmpty(response.ErrorMessage))
                            {
                                foreach (string url in response.Urls)
                                {
                                    dispatcher.Invoke((Action<string>) ((string iUrl) => _server.Urls.Add(iUrl)), url);
                                }
                            }
                            else
                            {
                                errorMessages.AppendLine(response.ErrorMessage);
                            }
                        } while (response.Next);
                    }
                    if (errorMessages.Length != 0)
                    {
                        dispatcher.Invoke(
                            (Action<string>) ((string errorMessage) => SUI.Dialogs.MessageBox.Show(errorMessage,
                                App.Name, MessageBoxButton.OK, MessageBoxImage.Warning)),
                            $"Правила были выполнены со следующими ошибками:{Environment.NewLine}{errorMessages}");
                    }
                }
                else
                {
                    dispatcher.Invoke(
                        (Action<string>) ((string errorMessage) =>
                            SUI.Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK,
                                MessageBoxImage.Error)), response.ErrorMessage);
                }
            }
        }
    }
}