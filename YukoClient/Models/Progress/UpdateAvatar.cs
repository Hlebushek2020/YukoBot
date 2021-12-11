using System;
using System.Windows;
using System.Windows.Threading;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;

namespace YukoClient.Models.Progress
{
    public class UpdateAvatar : UpdateBase
    {
        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Получение аватарки");
            UpdateAvatarResponse updateAvatarResponse = WebClient.UpdateAvatar();
            if (string.IsNullOrEmpty(updateAvatarResponse.ErrorMessage))
            {
                dispatcher.Invoke(() => State = "Обновление аватарки");
                if (!updateAvatarResponse.AvatarUri.Equals(Storage.Current.AvatarUri))
                {
                    Storage.Current.AvatarUri = updateAvatarResponse.AvatarUri;
                }
                base.Run(dispatcher);
            }
            else
            {
                dispatcher.Invoke((Action<string>)((string errorMessage) => UIC.MessageBox.Show(errorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error)), updateAvatarResponse.ErrorMessage);
            }
        }
    }
}
