using System;
using System.Windows;
using YukoClient.ViewModels;

namespace YukoClient
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            DataContext = new SettingsViewModel
            {
                Close = new Action(Close)
            };
        }
    }
}