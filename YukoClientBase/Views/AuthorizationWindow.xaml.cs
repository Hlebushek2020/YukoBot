using System.Security.Cryptography;
using System.Text;
using System.Windows;
using YukoClientBase.Models;
using YukoClientBase.ViewModels;

namespace YukoClientBase.Views
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow(BaseAuthorizationViewModel authorizationViewModel)
        {
            InitializeComponent();
            DataContext = authorizationViewModel;

            authorizationViewModel.SetCloseAction(Close);
            authorizationViewModel.SetGetPasswordFunc(() =>
            {
                if (!Settings.FakePassword.Equals(passwordBox_Password.Password))
                    return passwordBox_Password.Password;

                Settings.LoadLoginData(out _, out byte[] protectedData);
                return Encoding.UTF8.GetString(
                    ProtectedData.Unprotect(
                        protectedData,
                        null,
                        DataProtectionScope.CurrentUser));
            });

            Settings.LoadLoginData(out string login, out _);

            if (string.IsNullOrWhiteSpace(login))
                return;

            authorizationViewModel.Login = login;
            passwordBox_Password.Password = Settings.FakePassword;
            authorizationViewModel.IsRemember = true;

            authorizationViewModel.FullscreenEvent += AuthorizationViewModelOnFullscreenEvent;
        }

        private void AuthorizationViewModelOnFullscreenEvent()
        {
            if (WindowStyle == WindowStyle.None)
            {
                WindowState = WindowState.Normal;
                WindowStyle = WindowStyle.SingleBorderWindow;
            }
            else
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Maximized;
            }
        }
    }
}