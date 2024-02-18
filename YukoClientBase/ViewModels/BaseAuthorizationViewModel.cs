using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Responses;
using YukoClientBase.Views;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClientBase.ViewModels
{
    public abstract class BaseAuthorizationViewModel : BindableBase
    {
        #region Fields
        private Action _closeAction;
        private Func<string> _passwordFunc;
        private string _login;
        private bool _isRemember;
        #endregion

        #region Propirties
        public virtual string Title { get; }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                RaisePropertyChanged();
            }
        }

        public virtual ImageBrush Logo { get; }

        public bool IsRemember
        {
            get => _isRemember;
            set
            {
                _isRemember = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Commands
        public DelegateCommand LoginCommand { get; }
        public DelegateCommand SettingsCommand { get; }
        #endregion

        protected BaseAuthorizationViewModel(IUser store, WebClientBase webClient)
        {
            LoginCommand = new DelegateCommand(() =>
            {
                if (!Settings.Availability())
                {
                    MessageBox.Show("Сначала настройте программу! Значок в правом нижнем углу.", Title,
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(_passwordFunc?.Invoke()))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", Title, MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    AuthorizationResponse response = webClient.Authorization(Title, Login, _passwordFunc.Invoke());

                    if (IsRemember)
                    {
                        Settings.SaveLoginData(Login,
                            ProtectedData.Protect(
                                Encoding.UTF8.GetBytes(_passwordFunc.Invoke()),
                                null,
                                DataProtectionScope.CurrentUser));
                    }
                    else
                    {
                        Settings.DeleteLoginData();
                    }

                    store.AvatarUri = response.AvatarUri;
                    store.UserId = response.UserId;
                    store.Username = response.Username;

                    _closeAction?.Invoke();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            SettingsCommand = new DelegateCommand(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow(Title);
                settingsWindow.ShowDialog();
            });
        }

        public void SetCloseAction(Action closeAction) => _closeAction = closeAction;
        public void SetGetPasswordFunc(Func<string> passwordFunc) => _passwordFunc = passwordFunc;
    }
}