using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Models;

namespace YukoClient.Models.Progresses
{
    public class Download
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

            const string baseState = "Загрузка";

            progress.Report(new ProgressReportArgs
            {
                Maximum = _urls.Count,
                Minimum = 0,
                Value = 0,
                Text = baseState,
                IsIndeterminate = false
            });

            using (Downloader downloader = new Downloader(new DownloaderLogger(_folder)))
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

                    cancellationToken.ThrowIfCancellationRequested();

                    downloader.StartNew(url, fileNameFull, cancellationToken);

                    filesTemp.Clear();

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