using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using YukoClient.Models;
using YukoClient.Models.Web;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;
using YukoClientBase.Models.Web.Responses;
using SUI = Sergey.UI.Extension;

namespace YukoClient.ViewModels
{
    public class AuthorizationViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Propirties
        public string Title { get => App.Name; }
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
                    SUI.Dialogs.MessageBox.Show("Сначало настройте программу! Значек в правом нижнем углу.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password()))
                {
                    SUI.Dialogs.MessageBox.Show("Все поля должны быть заполнены!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                AuthorizationResponse response = WebClient.Current.Authorization(Login, Password());
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    SUI.Dialogs.MessageBox.Show(response.ErrorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    Storage.Current.AvatarUri = response.AvatarUri;
                    Storage.Current.Id = response.Id;
                    Storage.Current.Nikname = response.Nikname;
                    Close();
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