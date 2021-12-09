using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using YukoClient.Interfaces.ViewModel;
using YukoClient.Models;
using YukoClient.Models.Web;
using YukoClient.Models.Web.Responses;

namespace YukoClient.ViewModels
{
    public class AuthorizationViewModel : BindableBase, ICloseableView, ITitle
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
                    UIC.MessageBox.Show("Сначало настройте программу! Значек в правом нижнем углу.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password()))
                {
                    UIC.MessageBox.Show("Все поля должны быть заполнены!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                AuthorizationResponse response = WebClient.Authorization(Login, Password());
                if (!string.IsNullOrEmpty(response.ErrorMessage))
                {
                    UIC.MessageBox.Show(response.ErrorMessage, App.Name, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
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