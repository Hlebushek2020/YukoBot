using Newtonsoft.Json;
using System;
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
            string profileFile = Path.Combine(Settings.ProgramResourceFolder, "profile.json");
            if (File.Exists(profileFile))
            {
                dispatcher.Invoke(() => State = "Загрузка данных");
                string json = File.ReadAllText(profileFile, Encoding.UTF8);
                Storage newStore = JsonConvert.DeserializeObject<Storage>(json);
                Storage.Current.Id = newStore.Id;
                Storage.Current.AvatarUri = newStore.AvatarUri;
                Storage.Current.Nikname = newStore.Nikname;
                Storage.Current.Servers = newStore.Servers;
            }
            else
            {
                dispatcher.Invoke(() => State = "Получение данных");
                ClientDataResponse response = WebClient.Current.GetClientData();
                if (string.IsNullOrEmpty(response.ErrorMessage))
                {
                    Storage.Current.Id = response.Id;
                    Storage.Current.AvatarUri = response.AvatarUri;
                    Storage.Current.Nikname = response.Nikname;
                    Storage.Current.Servers = response.Servers;
                    Storage.Current.Save();
                }
                else
                {
                    dispatcher.Invoke((Action<string>)((string errorMessage) => Dialogs.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), response.ErrorMessage);
                }
            }
        }
    }
}