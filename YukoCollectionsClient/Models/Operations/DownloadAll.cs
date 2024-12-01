using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models;
using YukoClientBase.Models.Operations;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web.Providers;
using YWeb = YukoCollectionsClient.Models.Web;

namespace YukoCollectionsClient.Models.Operations
{
    public class DownloadAll : IOperation
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly ICollection<MessageCollection> _messageCollections;
        private readonly string _folder;
        private readonly bool _clearUrlList;

        public DownloadAll(ICollection<MessageCollection> messageCollections, string folder, bool clearUrlList)
        {
            _synchronizationContext = SynchronizationContext.Current;
            _messageCollections = messageCollections;
            _clearUrlList = clearUrlList;
            _folder = folder;
        }

        public async Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            foreach (MessageCollection collection in _messageCollections)
            {
                cancellationToken.ThrowIfCancellationRequested();

                progress.Report(new ProgressReportArgs { IsIndeterminate = true });

                if (_clearUrlList)
                    _synchronizationContext.Send(state => collection.Urls.Clear(), null);

                progress.Report(new ProgressReportArgs { Text = "Подключение" });

                using (UrlsProvider provider = YWeb.YukoWebClient.Current.GetUrls(
                           collection, out Response<BaseErrorJson> response))
                {
                    if (response.Error != null)
                        throw new ClientCodeException(response.Error.Code);

                    progress.Report(new ProgressReportArgs { Text = "Обработка" });

                    UrlsResponse urlsResponse;
                    do
                    {
                        urlsResponse = provider.ReadBlock();

                        foreach (string url in urlsResponse.Urls)
                            _synchronizationContext.Send(state => collection.Urls.Add(url), null);

                        MessageCollectionItem mcItem = collection.Items
                            .First(item => item.MessageId == urlsResponse.MessageId);
                        mcItem.IsChannelNotFound = urlsResponse.Error != null &&
                            urlsResponse.Error.Code == ClientErrorCodes.ChannelNotFound;
                        mcItem.IsMessageNotFound = urlsResponse.Error != null &&
                            urlsResponse.Error.Code == ClientErrorCodes.MessageNotFound;
                    } while (urlsResponse.Next && !cancellationToken.IsCancellationRequested);
                }

                cancellationToken.ThrowIfCancellationRequested();

                HashSet<string> filesTemp = new HashSet<string>();

                const string baseState = "Загрузка";

                progress.Report(new ProgressReportArgs
                {
                    Maximum = collection.Urls.Count,
                    Minimum = 0,
                    Value = 0,
                    Text = baseState,
                    IsIndeterminate = false
                });

                using (Downloader downloader = new Downloader(new DownloaderLogger(_folder)))
                {
                    foreach (string url in collection.Urls)
                    {
                        string baseFileName = Path.GetFileName(url);
                        if (baseFileName.Contains("?"))
                            baseFileName = baseFileName.Remove(baseFileName.IndexOf("?", StringComparison.Ordinal));

                        string fileNameFull = Path.Combine(_folder, baseFileName);
                        string fileName = baseFileName;

                        int i = 0;

                        while (File.Exists(fileNameFull) || filesTemp.Contains(fileName))
                        {
                            fileName = $"{i}-{baseFileName}";
                            fileNameFull = Path.Combine(_folder, fileName);
                            i++;
                        }

                        filesTemp.Add(fileName);

                        cancellationToken.ThrowIfCancellationRequested();

                        downloader.StartNew(url, fileNameFull, cancellationToken);

                        filesTemp.Clear();
                    }

                    int pointCount = 0;
                    int addPointTimer = 0;

                    while (downloader.IsActive)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await Task.Delay(100, cancellationToken);

                        progress.Report(new ProgressReportArgs { Value = downloader.Completed });

                        addPointTimer++;
                        if (addPointTimer < 10) continue;

                        addPointTimer = 0;

                        progress.Report(new ProgressReportArgs { Text = $"{baseState}{new string('.', pointCount)}" });

                        if (pointCount >= 3)
                            pointCount = -1;

                        pointCount++;
                    }
                }
            }
        }
    }
}