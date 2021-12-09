using System;
using System.Windows;
using YukoClient.ViewModels;

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
        }
    }
}