using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models;
using YukoClientBase.Models.Progresses;

namespace YukoCollectionsClient.Models.Progresses
{
    public class Download : BaseProgressModel
    {
        private readonly ICollection<string> _urls;
        private readonly string _folder;

        public Download(ICollection<string> urls, string folder)
        {
            _urls = urls;
            _folder = folder;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            HashSet<string> filesTemp = new HashSet<string>();
            Downloader downloader = new Downloader();

            const string baseState = "Загрузка";

            dispatcher.Invoke((Action<string, int>)((string state, int count) =>
            {
                MaxValue = count;
                State = state;
            }), baseState, _urls.Count);

            using (DownloaderLogger downloaderLogger = new DownloaderLogger(_folder))
            {
                foreach (string url in _urls)
                {
                    string baseFileName = Path.GetFileName(url);
                    if (baseFileName.Contains("?"))
                    {
                        baseFileName = baseFileName.Remove(baseFileName.IndexOf("?"));
                    }

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
                            dispatcher.Invoke((Action<string>)((string state) => State = state),
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