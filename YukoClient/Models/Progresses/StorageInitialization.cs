using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Exceptions;
using YukoClientBase.Models;
using YukoClientBase.Models.Progresses;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient.Models.Progress
{
    public class StorageInitialization : BaseProgressModel
    {
        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Поиск сохраненных данных");
            string serversCacheFilePath = Path.Combine(Settings.ProgramResourceFolder, Settings.ServersCacheFile);
            if (File.Exists(serversCacheFilePath))
            {
                dispatcher.Invoke(() => State = "Загрузка данных");
                string json = File.ReadAllText(serversCacheFilePath, Encoding.UTF8);
                Storage.Current.Servers = JsonConvert.DeserializeObject<ObservableCollection<Server>>(json);
            }
            else
            {
                try
                {
                    dispatcher.Invoke(() => State = "Получение данных");
                    ServersResponse response = WebClient.Current.GetServers();

                    if (response.Error != null)
                        throw new ClientCodeException(response.Error.Code);

                    Storage.Current.Servers = new ObservableCollection<Server>(response.Servers);
                }
                catch (Exception ex)
                {
                    dispatcher.Invoke((Action<string>) ((string errorMessage) =>
                            MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)),
                        ex.Message);
                }
            }
        }
    }
}