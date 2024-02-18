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
            authorizationViewModel.SetGetPasswordFunc(() => passwordBox_Password.Password);

            Settings.LoadLoginData(out string login, out byte[] protectedData);

            if (login == null)
                return;

            passwordBox_Password.Password =
                Encoding.UTF8.GetString(
                    ProtectedData.Unprotect(
                        protectedData,
                        null,
                        DataProtectionScope.CurrentUser));

            authorizationViewModel.IsRemember = true;
            authorizationViewModel.Login = login;
        }
    }
}