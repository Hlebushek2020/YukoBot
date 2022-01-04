using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Models;

namespace YukoClient.Models.Progress
{
    public class StorageInitialization : Base
    {
        public override void Run(Dispatcher dispatcher)
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
                dispatcher.Invoke(() => State = "Получение данных");
                ServersResponse response = WebClient.Current.GetServers();
                if (string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Storage.Current.Servers = new ObservableCollection<Server>(response.Servers);
                }
                else
                {
                    dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), response.ErrorMessage);
                }
            }
        }
    }
}