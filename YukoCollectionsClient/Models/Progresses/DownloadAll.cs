using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClientBase.Models;
using YukoClientBase.Models.Progresses;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web.Providers;
using SUI = Sergey.UI.Extension;
using YWeb = YukoCollectionsClient.Models.Web;

namespace YukoCollectionsClient.Models.Progress
{
    public class DownloadAll : BaseProgressModel
    {
        private readonly ICollection<MessageCollection> _messageCollections;
        private readonly string _folder;
        private readonly bool _clearUrlList;

        public DownloadAll(ICollection<MessageCollection> messageCollections, string folder, bool clearUrlList)
        {
            _messageCollections = messageCollections;
            _clearUrlList = clearUrlList;
            _folder = folder;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            foreach (MessageCollection collection in _messageCollections)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                dispatcher.Invoke(() =>
                {
                    IsIndeterminate = true;
                    Value = 0;
                });
                bool download = true;
                using (UrlsProvider provider = YWeb.WebClient.Current.GetUrls(collection))
                {
                    dispatcher.Invoke(() => State = "Аутентификация");
                    UrlsResponse response = provider.ReadBlock();
                    if (string.IsNullOrEmpty(response.ErrorMessage))
                    {
                        if (_clearUrlList)
                        {
                            dispatcher.Invoke(() => collection.Urls.Clear());
                        }
                        dispatcher.Invoke(
                            (Action<string>) ((string collectionName) =>
                                State = $"Обработка коллекции \"{collectionName}\""), collection.Name);
                        StringBuilder errorMessages = new StringBuilder();
                        do
                        {
                            response = provider.ReadBlock();
                            if (string.IsNullOrEmpty(response.ErrorMessage))
                            {
                                foreach (string url in response.Urls)
                                {
                                    dispatcher.Invoke(() => collection.Urls.Add(url));
                                }
                            }
                            else
                            {
                                errorMessages.AppendLine(response.ErrorMessage);
                            }
                        } while (response.Next && !cancellationToken.IsCancellationRequested);

                        if (cancellationToken.IsCancellationRequested)
                            break;

                        if (errorMessages.Length != 0)
                        {
                            MessageBoxResult messageBoxResult = (MessageBoxResult) dispatcher.Invoke(
                                (Func<string, MessageBoxResult>) ((string errorMessage) =>
                                    SUI.Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.YesNo,
                                        MessageBoxImage.Warning)),
                                $"Вы действительно хотите скачать вложения? При получении ссылок коллекции \"{collection.Name}\" возникли следующие ошибки:{Environment.NewLine}{errorMessages}");
                            download = messageBoxResult == MessageBoxResult.Yes;
                        }
                    }
                    else
                    {
                        dispatcher.Invoke(
                            (Action<string>) ((string errorMessage) => SUI.Dialogs.MessageBox.Show(errorMessage,
                                App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), response.ErrorMessage);
                        download = false;
                    }
                }
                if (download && !cancellationToken.IsCancellationRequested)
                {
                    HashSet<string> filesTemp = new HashSet<string>();
                    Downloader downloader = new Downloader();

                    const string baseState = "Загрузка";

                    dispatcher.Invoke((Action<string, int>) ((string state, int count) =>
                    {
                        MaxValue = count;
                        State = state;
                    }), baseState, collection.Urls.Count);

                    string folderName = collection.Name;
                    foreach (char replaceChar in Path.GetInvalidFileNameChars())
                    {
                        folderName = folderName.Replace(replaceChar.ToString(), "");
                    }
                    string collectionFolder = Path.Combine(_folder, folderName);
                    Directory.CreateDirectory(collectionFolder);

                    using (DownloaderLogger downloaderLogger = new DownloaderLogger(collectionFolder))
                    {
                        foreach (string url in collection.Urls)
                        {
                            string baseFileName = Path.GetFileName(url);
                            if (baseFileName.Contains("?"))
                            {
                                baseFileName = baseFileName.Remove(baseFileName.IndexOf("?"));
                            }

                            string fileNameFull = Path.Combine(collectionFolder, baseFileName);
                            string fileName = baseFileName;

                            int i = 0;

                            while (File.Exists(fileNameFull) || filesTemp.Contains(fileName))
                            {
                                fileName = $"{i}-{baseFileName}";
                                fileNameFull = Path.Combine(collectionFolder, fileName);
                                i++;
                            }

                            filesTemp.Add(fileName);

                            if (cancellationToken.IsCancellationRequested)
                                break;

                            downloader.StartNew(() =>
                            {
                                if (!cancellationToken.IsCancellationRequested)
                                {
                                    try
                                    {
                                        using (WebClient webClient = new WebClient())
                                        {
                                            webClient.DownloadFile(new Uri(url), fileNameFull);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        downloaderLogger.Log(url, ex);
                                    }
                                    dispatcher.Invoke(() => Value++);
                                }
                            });
                        }

                        filesTemp.Clear();

                        int addPointTimer = 0;
                        int pointCount = 0;

                        while (downloader.IsActive)
                        {
                            Thread.Sleep(100);
                            if (!cancellationToken.IsCancellationRequested)
                            {
                                addPointTimer++;
                                if (addPointTimer >= 9)
                                {
                                    addPointTimer = 0;
                                    dispatcher.Invoke((Action<string>) ((string state) => State = state),
                                        $"{baseState} {new string('.', pointCount)}");
                                    if (pointCount >= 3)
                                    {
                                        pointCount = -1;
                                    }
                                    pointCount++;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}