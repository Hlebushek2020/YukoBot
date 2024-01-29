using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using YukoClient.ViewModels;
using YukoClientBase.Models;

namespace YukoClient
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationWindow.xaml
    /// </summary>
    public partial class AuthorizationWindow : Window
    {
        public AuthorizationWindow()
        {
            InitializeComponent();
            DataContext = new AuthorizationViewModel
            {
                Close = new Action(Close),
                Password = () => passwordBox_Password.Password
            };

            Settings.LoadLoginData(out string login, out byte[] protectedData);

            if (login == null)
                return;

            passwordBox_Password.Password =
                Encoding.UTF8.GetString(
                    ProtectedData.Unprotect(
                        protectedData,
                        null,
                        DataProtectionScope.CurrentUser));

            ((AuthorizationViewModel)DataContext).Login = login;
        }
    }
}