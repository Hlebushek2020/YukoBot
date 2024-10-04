using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Models;

namespace YukoClient.Models.Progresses
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

        public async Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            HashSet<string> filesTemp = new HashSet<string>();
            Downloader downloader = new Downloader();

            const string baseState = "Загрузка";

            progress.Report(new ProgressReportArgs
            {
                Maximum = _urls.Count,
                Minimum = 0,
                Value = 0,
                Text = baseState,
                IsIndeterminate = false
            });

            using (DownloaderLogger downloaderLogger = new DownloaderLogger(_folder))
            {
                foreach (string url in _urls)
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

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    downloader.StartNew(() =>
                    {
                        try
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            using (WebClient webClient = new WebClient())
                                webClient.DownloadFile(new Uri(url), fileNameFull);
                        }
                        catch (OperationCanceledException) { }
                        catch (Exception ex)
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            downloaderLogger.Log(url, ex);
                        }

                        dispatcher.Invoke(() => Value++);
                    });

                    filesTemp.Clear();

                    int pointCount = 0;

                    while (downloader.IsActive)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        await Task.Delay(100, cancellationToken);

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