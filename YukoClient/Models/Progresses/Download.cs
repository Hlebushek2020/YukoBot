using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models;

namespace YukoClient.Models.Progress
{
    public class Download : Base
    {
        private readonly ICollection<string> urls;
        private readonly string folder;

        public Download(ICollection<string> urls, string folder)
        {
            this.urls = urls;
            this.folder = folder;
        }

        public override void Run(Dispatcher dispatcher)
        {
            HashSet<string> filesTemp = new HashSet<string>();
            Downloader downloader = new Downloader();

            string baseState = "Загрузка";

            dispatcher.Invoke((Action<string, int>)((string state, int count) =>
            {
                MaxValue = count;
                State = state;
            }), baseState, urls.Count);


            foreach (string url in urls)
            {
                string baseFileName = Path.GetFileName(url);
                if (baseFileName.Contains("?"))
                {
                    baseFileName = baseFileName.Remove(baseFileName.IndexOf("?"));
                }

                string fileNameFull = Path.Combine(folder, baseFileName);
                string fileName = baseFileName;

                int i = 0;

                while (File.Exists(fileNameFull) || filesTemp.Contains(fileName))
                {
                    fileName = $"{i}-{baseFileName}";
                    fileNameFull = Path.Combine(folder, fileName);
                    i++;
                }

                filesTemp.Add(fileName);

                downloader.StartNew(() => DownloadFile(url, fileNameFull, dispatcher));
            }

            filesTemp.Clear();

            int addPointTimer = 0;
            int pointCount = 0;

            while (downloader.CompletedCount < urls.Count)
            {
                Thread.Sleep(100);
                addPointTimer++;
                if (addPointTimer >= 9)
                {
                    addPointTimer = 0;
                    dispatcher.Invoke((Action<string>)((string state) => State = state), $"{baseState} {new string('.', pointCount)}");
                    if (pointCount >= 3)
                    {
                        pointCount = -1;
                    }
                    pointCount++;
                }
            };
        }

        private void DownloadFile(string url, string fileName, Dispatcher dispatcher)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    webClient.DownloadFile(new Uri(url), fileName);
                }
            }
            catch { }
            dispatcher.Invoke(() => Value++);
        }
    }
}