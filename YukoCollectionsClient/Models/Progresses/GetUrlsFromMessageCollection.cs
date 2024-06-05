using System;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Progresses;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Providers;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoCollectionsClient.Models.Progresses
{
    public class GetUrlsFromMessageCollection : BaseProgressModel
    {
        private readonly MessageCollection _messageCollection;

        public GetUrlsFromMessageCollection(MessageCollection messageCollection)
        {
            _messageCollection = messageCollection;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            try
            {
                dispatcher.Invoke(() => State = "Подключение");
                using (UrlsProvider provider = WebClient.Current.GetUrls(
                           _messageCollection, out Response<BaseErrorJson> response))
                {
                    if (response.Error != null)
                        throw new ClientCodeException(response.Error.Code);

                    dispatcher.Invoke(() => State = "Обработка");
                    UrlsResponse urlsResponse;
                    do
                    {
                        urlsResponse = provider.ReadBlock();

                        foreach (string url in urlsResponse.Urls)
                            dispatcher.Invoke(() => _messageCollection.Urls.Add(url));

                        MessageCollectionItem mcItem = _messageCollection.Items
                            .First(item => item.MessageId == urlsResponse.MessageId);
                        mcItem.IsChannelNotFound = urlsResponse.Error != null &&
                                                   urlsResponse.Error.Code == ClientErrorCodes.ChannelNotFound;
                        mcItem.IsMessageNotFound = urlsResponse.Error != null &&
                                                   urlsResponse.Error.Code == ClientErrorCodes.MessageNotFound;
                    } while (urlsResponse.Next && !cancellationToken.IsCancellationRequested);
                }
            }
            catch (Exception ex)
            {
                dispatcher.Invoke((Action<string>)((string errorMessage) =>
                    MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), ex.Message);
            }
        }
    }
}