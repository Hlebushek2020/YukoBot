using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Mvvm;
using YukoClient.Models;
using YukoClient.Models.Web;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;
using YukoClientBase.Models.Web.Responses;
using YukoClientBase.ViewModels.Interfaces;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient.ViewModels
{
    public class AuthorizationViewModel : BindableBase, IAuthorizationViewModel, ICloseableView, IViewTitle
    {
        #region Fields
        private Func<string> _passwordFunc;
        private string _login;
        private bool _isRemember;
        #endregion

        #region Propirties
        public string Title => App.Name;
        public Action Close { get; set; }

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                RaisePropertyChanged();
            }
        }

        public ImageBrush Logo
        {
            get
            {
                BitmapDecoder decoder = BitmapDecoder.Create(
                    new Uri("pack://application:,,,/Resources/program-icon.ico"),
                    BitmapCreateOptions.DelayCreation,
                    BitmapCacheOption.OnDemand);

                BitmapFrame bitmapFrame = decoder.Frames.Where(f => f.Width <= 256)
                        .OrderByDescending(f => f.Width).FirstOrDefault() ??
                    decoder.Frames.OrderBy(f => f.Width).FirstOrDefault();

                return new ImageBrush { Stretch = Stretch.Uniform, ImageSource = bitmapFrame };
            }
        }

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

                if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(_passwordFunc?.Invoke()))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", App.Name, MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                try
                {
                    AuthorizationResponse response = WebClient.Current.Authorization(Login, _passwordFunc.Invoke());

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

                    Storage.Current.AvatarUri = response.AvatarUri;
                    Storage.Current.UserId = response.UserId;
                    Storage.Current.Username = response.Username;

                    Close?.Invoke();
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

        public void SetCloseAction(Action closeAction) => Close = closeAction;
        public void SetGetPasswordFunc(Func<string> passwordFunc) => _passwordFunc = passwordFunc;
    }
}