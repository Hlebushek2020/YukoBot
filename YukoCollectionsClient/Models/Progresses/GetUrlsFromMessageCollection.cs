using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Providers;
using SUI = Sergey.UI.Extension;

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
            using (UrlsProvider provider = WebClient.Current.GetUrls(messageCollection.Items))
            {
                dispatcher.Invoke(() => State = "Аутентификация");
                UrlsResponse response = provider.ReadBlock();
                if (string.IsNullOrEmpty(response.ErrorMessage))
                {
                    dispatcher.Invoke(() => State = "Обработка");
                    StringBuilder errorMessages = new StringBuilder();
                    do
                    {
                        response = provider.ReadBlock();
                        if (string.IsNullOrEmpty(response.ErrorMessage))
                        {
                            foreach (string url in response.Urls)
                            {
                                dispatcher.Invoke(() => messageCollection.Urls.Add(url));
                            }
                        }
                        else
                        {
                            errorMessages.AppendLine(response.ErrorMessage);
                        }

                    } while (response.Next);
                    if (errorMessages.Length != 0)
                    {
                        dispatcher.Invoke((Action<string>)((string errorMessage) => SUI.Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Warning)), $"При получении ссылок возникли следующие ошибки:{Environment.NewLine}{errorMessages}");
                    }
                }
                else
                {
                    dispatcher.Invoke((Action<string>)((string errorMessage) => SUI.Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), response.ErrorMessage);
                }
            }
        }
    }
}