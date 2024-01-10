using System;
using System.Windows;
using Prism.Commands;
using Prism.Mvvm;
using YukoClientBase.Exceptions;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models;
using YukoCollectionsClient.Models.Web;
using SUI = Sergey.UI.Extension;

namespace YukoCollectionsClient.ViewModels
{
    public class AuthorizationViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Propirties
        public string Title
        {
            get => App.Name;
        }

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
                    SUI.Dialogs.MessageBox.Show("Сначала настройте программу! Значок в правом нижнем углу.", App.Name,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password()))
                {
                    SUI.Dialogs.MessageBox.Show("Все поля должны быть заполнены!", App.Name, MessageBoxButton.OK,
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