using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using YukoClient.Models;
using YukoClient.Models.Web;
using YukoClientBase.Exceptions;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;
using YukoClientBase.Models.Web.Responses;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient.ViewModels
{
    public class AuthorizationViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Propirties
        public string Title => App.Name;
        public Action Close { get; set; }
        public string Login { get; set; }
        public Func<string> Password { get; set; }
        #endregion

        #region Commands
        public DelegateCommand LoginCommand { get; }
        public DelegateCommand SettingsCommand { get; }
        #endregion

        public AuthorizationViewModel()
        {
            LoginCommand = new DelegateCommand(() =>
            {
                if (!Settings.Availability())
                {
                    MessageBox.Show("Сначала настройте программу! Значок в правом нижнем углу.", App.Name,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password()))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", App.Name, MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    AuthorizationResponse response = WebClient.Current.Authorization(Login, Password());

                    if (response.Error != null)
                        throw new ClientCodeException(response.Error.Code);

                    Storage.Current.AvatarUri = response.AvatarUri;
                    Storage.Current.UserId = response.UserId;
                    Storage.Current.Username = response.Username;

                    Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, App.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            SettingsCommand = new DelegateCommand(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            });
        }
    }
}