using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Providers;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Progress
{
    public class GetUrlsFromMessageCollection : Base
    {
        private readonly MessageCollection messageCollection;

        public GetUrlsFromMessageCollection(MessageCollection messageCollection)
        {
            this.messageCollection = messageCollection;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Подключение");
            using (GetUrlsProvider provider = WebClient.Current.GetUrls(messageCollection.Items))
            {
                dispatcher.Invoke(() => State = "Аутентификация");
                GetUrlsResponse response = provider.ReadBlock();
                if (string.IsNullOrEmpty(response.ErrorMessage))
                {
                    StringBuilder errorMessages = new StringBuilder();
                    do
                    {
                        response = provider.ReadBlock();
                        if (string.IsNullOrEmpty(response.ErrorMessage))
                        {
                            foreach (string url in response.Urls)
                            {
                                messageCollection.Urls.Add(url);
                            }
                        }
                        else
                        {
                            errorMessages.AppendLine();
                        }

                    } while (response.Next);
                    if (errorMessages.Length != 0)
                    {
                        dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Warning)), $"Правила были выполнены со следующими ошибками:{Environment.NewLine}{errorMessages}");
                    }
                }
                else
                {
                    dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), response.ErrorMessage);
                }
            }
        }
    }
}