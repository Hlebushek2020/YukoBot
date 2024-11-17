using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Args;
using YukoClientBase.Exceptions;
using YukoClientBase.Models;
using YukoClientBase.Models.Operations;

namespace YukoClient.Models.Operations
{
    public class StorageInitialization : IOperation
    {
        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Поиск сохраненных данных" });
            string serversCacheFilePath = Path.Combine(Settings.ProgramResourceFolder, Settings.ServersCacheFile);
            if (File.Exists(serversCacheFilePath))
            {
                progress.Report(new ProgressReportArgs { Text = "Загрузка данных" });
                string json = File.ReadAllText(serversCacheFilePath, Encoding.UTF8);
                Storage.Current.Servers = JsonConvert.DeserializeObject<ObservableCollection<Server>>(json);
            }
            else
            {
                progress.Report(new ProgressReportArgs { Text = "Получение данных" });
                ServersResponse response = WebClient.Current.GetServers();

                if (response.Error != null)
                    throw new ClientCodeException(response.Error.Code);

                Storage.Current.Servers = new ObservableCollection<Server>(response.Servers);
            }

            return Task.CompletedTask;
        }
    }
}