using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Operations;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Providers;

namespace YukoCollectionsClient.Models.Operations
{
    public class GetUrlsFromMessageCollection : IOperation
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly MessageCollection _messageCollection;

        public GetUrlsFromMessageCollection(MessageCollection messageCollection)
        {
            _synchronizationContext = SynchronizationContext.Current;
            _messageCollection = messageCollection;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { IsIndeterminate = true, Text = "Подключение" });
            using (UrlsProvider provider = YukoWebClient.Current.GetUrls(
                       _messageCollection, out Response<BaseErrorJson> response))
            {
                if (response.Error != null)
                    throw new ClientCodeException(response.Error.Code);

                progress.Report(new ProgressReportArgs { Text = "Обработка" });
                UrlsResponse urlsResponse;
                do
                {
                    urlsResponse = provider.ReadBlock();

                    foreach (string url in urlsResponse.Urls)
                        _synchronizationContext.Send(state => _messageCollection.Urls.Add(url), null);

                    MessageCollectionItem mcItem = _messageCollection.Items
                        .First(item => item.MessageId == urlsResponse.MessageId);
                    mcItem.IsChannelNotFound = urlsResponse.Error != null &&
                        urlsResponse.Error.Code == ClientErrorCodes.ChannelNotFound;
                    mcItem.IsMessageNotFound = urlsResponse.Error != null &&
                        urlsResponse.Error.Code == ClientErrorCodes.MessageNotFound;
                } while (urlsResponse.Next && !cancellationToken.IsCancellationRequested);
            }

            return Task.CompletedTask;
        }
    }
}