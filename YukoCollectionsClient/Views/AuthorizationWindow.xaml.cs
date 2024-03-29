﻿using System;
using System.Windows;
using YukoCollectionsClient.ViewModels;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for AuthorizationWindow.xaml
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
